using IngematicaAngularBase.Dal;
using IngematicaAngularBase.Model;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.Entities;
using IngematicaAngularBase.Model.ViewModels;
using Ingematica.MailService.Common;
using Ingematica.MailService.Contract;
using Ingematica.MailService.Service;
using Ingematica.MailService.Service.WCF;
using OAuth.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using netDxf;
using netDxf.Entities;

namespace IngematicaAngularBase.Bll.Common
{
    public class Optimizer
    {
        private DxfDocument initialFlat;
        private List<Mueble> muebleList;
        private List<OptimizacionMueble> muebleCantidadList;
        private bool optimizarCosto;
        private decimal costoMaximo;
        private string escala;

        public Optimizer(DxfDocument dxfDocument, bool optimizarCostoParam, decimal costoMaximoParam, List<OptimizacionMueble> muebleCantidadListParam, List<Mueble> muebleListParam, string escalaParam)
        {
            this.initialFlat = dxfDocument;
            this.optimizarCosto = optimizarCostoParam;
            this.costoMaximo = costoMaximoParam;
            this.muebleCantidadList = muebleCantidadListParam;
            this.muebleList = muebleListParam;
            this.escala = escalaParam;
        }


        #region
        //Agregar consideracion de la escala del plano. Los muebles los tomamos en metros en el momento del alta. Al final.
        #endregion
        public DxfDocument Generate()
        {
            #region
            // validar cantidad de personas por metro cuadrado. Si hay de mas => lanzo excepcion.
            //Considero que la cantida de muebles solicitados es para el total de regiones que esten en el dxf
            #endregion

            DxfDocument finalFlat = new DxfDocument();
            List<LwPolylineVertex> vertices = new List<LwPolylineVertex>();
            double anchoPasillos = 0; 
            Celda celda = new Celda();
            List<Mueble> muebleListTemp;
            bool hayEspacio;//Se usa dsp de la compactacion
            int index;

            foreach(var sentido in Enum.GetValues(typeof(SentidoPasillosEnum)))
            {
                //Modificar esta lista. Cada mueble se tiene que repetir la cantidad de veces que este en muebleCantidadList
                muebleListTemp = new List<Mueble>(muebleList).OrderBy(x => x.OrdenDePrioridad).ToList();

                foreach (LwPolyline lwPolyline in initialFlat.LwPolylines)
                {
                    anchoPasillos = GetAnchoPasillo(lwPolyline, (int)sentido); //TODO
                    vertices = lwPolyline.Vertexes;
                    hayEspacio = true;
                    List<AreaOptimizacion> areaOptmizacion = GetSubAreas(vertices);

                    while (muebleListTemp.Count > 0 && hayEspacio)
                    {
                        celda = GetTamañoMaximoCelda(muebleListTemp, (int)sentido);
                        index = UbicarMuebles(areaOptmizacion, muebleListTemp, (int)sentido, anchoPasillos, celda);

                        if (index == 0)
                            break;

                        hayEspacio = false;
                        //hayEspacio = Compactar(areaOptmizacion, sentido) -> Compacta la lista de zonasOcupadas y retorna si queda lugar libre en algun subarea
                    }
                }
            }

            return null;
        }

        #region comment 
        //Elimina los muebles de muebleListTemp. Ubica muebles x fila + pasillo 
        #endregion
        public int UbicarMuebles(List<AreaOptimizacion> areaOptimizacion, List<Mueble> muebleList, int sentido, double anchoPasillos, Celda celda)
        {
            #region comment
            //verifico de todas las sub-areas donde puedo ingresar los muebles
            //posible bug => Que haya espacio en las areas pero ninguno sea menor o igual al tamaño de la celda.
            #endregion 

            int index = GetSubAreaConEspacio(areaOptimizacion, celda);

            if (index == 0)
                return index;

            if(sentido == (int)SentidoPasillosEnum.Largo)
            {
                #region
                //obtengo el ultimo mueble. Verifico si hay lugar en esa fila, sino uso la siguiente si es que entra
                #endregion
                UbicarMueblesEnLargo(areaOptimizacion[index], muebleList, sentido, anchoPasillos, celda);

            } else if (sentido == (int)SentidoPasillosEnum.Ancho)
            {
                UbicarMueblesEnAncho(areaOptimizacion[index], muebleList, sentido, anchoPasillos, celda);
            }

            return index;
        }

        #region
        //Largo en Y
        //1. Obtengo el ultimo mueble. Valido si queda lugar en esa fila
        //1.1 Si hay lugar => incerto en esa fila y agrego un pasillo
        //2. Sino => Creo una nueva fila de celdas al lado. La cargo. Si completo la fila inserto un pasillo.
        #endregion
        public void UbicarMueblesEnLargo(AreaOptimizacion areaOptimizacion, List<Mueble> muebleList, int sentido, double anchoPasillos, Celda celda)
        {
            List<MueblesOptmizacion> celdaList = new List<MueblesOptmizacion>();

            Model.ViewModels.Vector2 izquierdaAbajo = new Model.ViewModels.Vector2();
            Model.ViewModels.Vector2 derechaAbajo = new Model.ViewModels.Vector2();
            double xMax;
            double xMaxArea;

            xMaxArea = areaOptimizacion.VerticeDerechaAbajo.X;
            xMax = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();
            derechaAbajo = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo).Where(x => x.X == xMax).Min();
            izquierdaAbajo = areaOptimizacion.MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeIzquierdaAbajo).First();

            #region
            //Completo la fila incompleta
            #endregion
            if (derechaAbajo.Y > areaOptimizacion.VerticeDerechaAbajo.Y) { 

                celdaList = GetCeldaList(xMaxArea, celda, izquierdaAbajo, areaOptimizacion.VerticeDerechaAbajo.Y);

                #region
                //verificar que muebleList y areaOptimizacion se esten pasando por referencia.
                #endregion
                foreach (MueblesOptmizacion celdaTemp in celdaList)
                    IncertarMuebleCelda(celdaTemp, muebleList, areaOptimizacion);

                xMax = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();

                InsertarPasilloEnLargo(areaOptimizacion, anchoPasillos, xMax);
            }

            #region
            //si no valido esto pueden quedar 2 pasillos contiguos
            #endregion
            if (muebleList.Count > 0)
            {
                do
                {

                    xMax = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();

                    #region
                    //Genera una nueva fila de celdas
                    //usa xMax porque en caso de que quede formato cerrucho x compactacion, se desprecia una pequeña parte del plano
                    #endregion
                    celdaList = GetCeldaList(xMaxArea, celda, xMax, areaOptimizacion);

                    #region
                    //tanto GetCeldaList como InsertarPasillo validan no pasarse del limite del subarea
                    #endregion
                    foreach (MueblesOptmizacion celdaTemp in celdaList)
                        IncertarMuebleCelda(celdaTemp, muebleList, areaOptimizacion);

                    xMax = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();

                    InsertarPasilloEnLargo(areaOptimizacion, anchoPasillos, xMax);

                } while (muebleList.Count > 0 && celdaList.Count > 0);
                #region
                //con celdaList > 0 valido que se haya incertado algo en la iteracion anterior
                #endregion
            }
        }

        public void InsertarPasilloEnLargo(AreaOptimizacion areaOptimizacion, double anchoPasillos, double xMax)
        {
            if(xMax + anchoPasillos <= areaOptimizacion.VerticeDerechaAbajo.X)
            {
                MueblesOptmizacion pasillo = new MueblesOptmizacion();
                pasillo.Mueble = new Mueble();

                Model.ViewModels.Vector2 VerticeIzquierdaArriba = new Model.ViewModels.Vector2();
                VerticeIzquierdaArriba.Y = areaOptimizacion.VerticeIzquierdaArriba.Y;
                VerticeIzquierdaArriba.X = xMax;
                pasillo.VerticeIzquierdaArriba = VerticeIzquierdaArriba;

                Model.ViewModels.Vector2 VerticeDerechaArriba = new Model.ViewModels.Vector2();
                VerticeDerechaArriba.Y = areaOptimizacion.VerticeDerechaArriba.Y;
                VerticeDerechaArriba.X = xMax + anchoPasillos;
                pasillo.VerticeDerechaArriba = VerticeDerechaArriba;

                Model.ViewModels.Vector2 VerticeIzquierdaAbajo = new Model.ViewModels.Vector2();
                VerticeIzquierdaAbajo.Y = areaOptimizacion.VerticeIzquierdaAbajo.Y;
                VerticeIzquierdaAbajo.X = xMax;
                pasillo.VerticeIzquierdaAbajo = VerticeIzquierdaAbajo;

                Model.ViewModels.Vector2 VerticeDerechaAbajo = new Model.ViewModels.Vector2();
                VerticeDerechaAbajo.Y = areaOptimizacion.VerticeDerechaAbajo.Y;
                VerticeDerechaAbajo.X = xMax + anchoPasillos;
                pasillo.VerticeDerechaAbajo = VerticeDerechaAbajo;

                areaOptimizacion.MueblesList.Add(pasillo);
            }
        }

        public void IncertarMuebleCelda(MueblesOptmizacion celda, List<Mueble> muebleList, AreaOptimizacion areaOptimizacion)
        {
            if (muebleList.Any())
            {
                celda.Mueble = muebleList.First();
                areaOptimizacion.MueblesList.Add(celda);
                muebleList.RemoveAt(0);
            }
        }

        public List<MueblesOptmizacion> GetCeldaList(double xMaxArea, Celda celda, double xMax, AreaOptimizacion areaOptimizacion)
        {
            return null;
        }

        #region
        //Recibe el xMaxArea, este caso se da solo despues de una compactacion => el tamaño de la celda puede ser distinto al que se venia trabajando
        //en la iteracion anterior => tengo que validar no excederme de las dimenciones del plano.
        #endregion
        public List<MueblesOptmizacion> GetCeldaList(double xMaxArea, Celda celda, Model.ViewModels.Vector2 izquierdaAbajo, double limiteInferior)
        {
            List<MueblesOptmizacion> celdaList = new List<MueblesOptmizacion>();

            while(izquierdaAbajo.Y + celda.Largo >= limiteInferior) {

                if (izquierdaAbajo.X + celda.Ancho <= xMaxArea) {

                    MueblesOptmizacion celdaMueble = new MueblesOptmizacion();
                    celdaMueble.VerticeIzquierdaArriba = izquierdaAbajo;

                    Model.ViewModels.Vector2 VerticeDerechaArriba = new Model.ViewModels.Vector2();
                    VerticeDerechaArriba.Y = celdaMueble.VerticeIzquierdaArriba.Y;
                    VerticeDerechaArriba.X = celdaMueble.VerticeIzquierdaArriba.X + celda.Ancho;
                    celdaMueble.VerticeDerechaArriba = VerticeDerechaArriba;


                    Model.ViewModels.Vector2 VerticeIzquierdaAbajo = new Model.ViewModels.Vector2();
                    VerticeIzquierdaAbajo.Y = celdaMueble.VerticeIzquierdaArriba.Y - celda.Largo;
                    VerticeIzquierdaAbajo.X = celdaMueble.VerticeIzquierdaArriba.X;
                    celdaMueble.VerticeIzquierdaAbajo = VerticeIzquierdaAbajo;

                    Model.ViewModels.Vector2 VerticeDerechaAbajo = new Model.ViewModels.Vector2();
                    VerticeDerechaAbajo.Y = celdaMueble.VerticeIzquierdaAbajo.Y;
                    VerticeDerechaAbajo.X = celdaMueble.VerticeDerechaArriba.X;
                    celdaMueble.VerticeDerechaAbajo = VerticeDerechaAbajo;

                    celdaMueble.Mueble = new Mueble();
                    celdaMueble.Largo = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                    celdaMueble.Ancho = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
                    celdaMueble.Area = celdaMueble.Largo * celdaMueble.Ancho;

                    celdaList.Add(celdaMueble);

                    izquierdaAbajo = celdaMueble.VerticeIzquierdaAbajo;
                }
                else
                {
                    #region
                    //En caso de que me pase del X maximo de la subarea.
                    #endregion
                    break;
                }
            }
            return celdaList;
        }

        public List<Celda> GetCeldaList(double xMaxArea, Celda celda)
        {
            return null;
        }

        public void UbicarMueblesEnAncho(AreaOptimizacion areaOptimizacion, List<Mueble> muebleList, int sentido, double anchoPasillos, Celda celda)
        {

        }

        #region
        //Obtengo el mueble que tenga las mayores coordenadas y verifico si coinciden con el ultimo vertice
        //del subarea y que el area de la celda sea menor al lugar vacion.
        //Seguro va a haber alguna porque ya retorno en la compactacion si hay queda espacio en algun area.
        #endregion
        public int GetSubAreaConEspacio(List<AreaOptimizacion> areaOptimizacion, Celda celda)
        {
            MueblesOptmizacion muebleOptimizacion = new MueblesOptmizacion();
            Model.ViewModels.Vector2 izquierdaArriba = new Model.ViewModels.Vector2();
            Model.ViewModels.Vector2 derechaArriba = new Model.ViewModels.Vector2();
            Model.ViewModels.Vector2 izquierdaAbajo = new Model.ViewModels.Vector2();
            Model.ViewModels.Vector2 derechaAbajo = new Model.ViewModels.Vector2();
            double xMax;

            for (int i = 0; i < areaOptimizacion.Count; i++)
            {
                if (areaOptimizacion[i].MueblesList.Any())
                {
                    xMax = areaOptimizacion[i].MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();
                    derechaAbajo = areaOptimizacion[i].MueblesList.Select(x => x.VerticeDerechaAbajo).Where(x=> x.X == xMax).Min();
                    derechaArriba = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeDerechaArriba).First();
                    izquierdaAbajo = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeIzquierdaAbajo).First();
                    izquierdaArriba = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeIzquierdaArriba).First();
                    
                    //Si coinciden los vertices => el area esta llena => paso a la siguiente area.
                    if (derechaAbajo == areaOptimizacion[i].VerticeDerechaAbajo)
                        continue;

                    if ((areaOptimizacion[i].Area - areaOptimizacion[i].MueblesList.Sum(x => x.Area)) > (celda.Ancho * celda.Largo))
                        return i;
                }
                else
                {
                    //si el area no tiene muebles => esta vacia, asi que la retorno.
                    return i;
                }
                
            }

            return 0;
        }

        #region
        //Metodo que retorna el tamaño de la celda.
        //Recibe el sentido en que se ponen los muebles, porque si algun mueble posee radio mayor, 
        //se considera en el sentido de recorrido del plano.
        #endregion
        public Celda GetTamañoMaximoCelda(List<Mueble> muebles, int sentido)
        {
            Celda celda = new Celda();
            decimal? Largo = 0;
            decimal? Ancho = 0;
            decimal? RadioMayor = 0;
            decimal? RadioMenor = 0;

            Largo = muebles.Select(x => x.Largo + x.DistanciaParedes + x.DistanciaProximoMueble).Max();
            Ancho = muebles.Select(x => x.Ancho + x.DistanciaParedes + x.DistanciaProximoMueble).Max();
            RadioMayor = muebles.Select(x => x.RadioMayor + x.DistanciaParedes + x.DistanciaProximoMueble).Max();
            RadioMenor = muebles.Select(x => x.RadioMayor + x.DistanciaParedes + x.DistanciaProximoMueble).Max();

            switch (sentido){

                case 1:

                    if (RadioMayor != null && Largo != null && RadioMayor > Largo)
                        celda.Largo = (double)RadioMayor;
                    else if(Largo != null)
                        celda.Largo = (double)Largo;
                    else
                        celda.Largo = (double)RadioMayor;

                    if (RadioMenor != null && Ancho != null && RadioMenor > Ancho)
                        celda.Ancho = (double)RadioMenor;
                    else if (Ancho != null)
                        celda.Ancho = (double)Ancho;
                    else
                        celda.Ancho = (double)RadioMenor;

                    break;

                case 2:

                    if (RadioMayor != null && Ancho != null && RadioMayor > Ancho)
                        celda.Ancho = (double)RadioMayor;
                    else if (Ancho != null)
                        celda.Ancho = (double)Ancho;
                    else
                        celda.Ancho = (double)RadioMayor;

                    if (RadioMenor != null && Largo != null && RadioMenor > Largo)
                        celda.Largo = (double)RadioMenor;
                    else if (Largo != null)
                        celda.Largo = (double)Largo;
                    else
                        celda.Largo = (double)RadioMenor;

                    break;

                //case 3:   TODO
            }

            return celda;
        }

        public List<AreaOptimizacion> GetSubAreas(List<LwPolylineVertex> vertices)
        {
            double maximoY = vertices.Select(x => x.Position.Y).Max();
            double maximoX = vertices.Select(x => x.Position.X).Max();
            double minimoY = vertices.Select(x => x.Position.Y).Min();

            netDxf.Vector2 derechaArriba = vertices
                                            .Select(coordenadas => coordenadas.Position)
                                            .Where(coordenadas => coordenadas.X == maximoX && coordenadas.Y == maximoY)
                                            .First();

            netDxf.Vector2 IzquierdaArriba = vertices
                                                .Select(coordenadas => coordenadas.Position)
                                                .Where(coordenadas => coordenadas.X != maximoX && coordenadas.Y == maximoY)
                                                .First();

            netDxf.Vector2 derechaAbajo = vertices
                                            .Select(coordenadas => coordenadas.Position)
                                            .Where(coordenadas => coordenadas.X == maximoX && coordenadas.Y == minimoY)
                                            .First();

            netDxf.Vector2 izquierdaAbajo = vertices
                                                .Select(coordenadas => coordenadas.Position)
                                                .Where(coordenadas => coordenadas.X != maximoX && coordenadas.Y == minimoY)
                                                .First();

            //Estos 2 pueden no estar
            netDxf.Vector2 centralTemporalUno = vertices
                                                .Select(coordenadas => coordenadas.Position)
                                                .Where(coordenadas => coordenadas != derechaArriba &&
                                                                      coordenadas != derechaAbajo &&
                                                                      coordenadas != IzquierdaArriba &&
                                                                      coordenadas != izquierdaAbajo)
                                                .FirstOrDefault();

            netDxf.Vector2 centralTemporalDos = vertices
                                    .Select(coordenadas => coordenadas.Position)
                                    .Where(coordenadas => coordenadas != derechaArriba &&
                                                          coordenadas != derechaAbajo &&
                                                          coordenadas != IzquierdaArriba &&
                                                          coordenadas != izquierdaAbajo &&
                                                          coordenadas != centralTemporalUno)
                                    .FirstOrDefault();

            netDxf.Vector2 centralMax;
            netDxf.Vector2 centralMin;

            if ((centralTemporalUno.X != 0 || centralTemporalUno.Y != 0) 
                && (centralTemporalDos.X != 0 || centralTemporalDos.Y != 0) 
                && centralTemporalUno.X > centralTemporalDos.X)
            {
                centralMax = centralTemporalUno;
                centralMin = centralTemporalDos;
            }
            else
            {
                centralMax = centralTemporalDos;
                centralMin = centralTemporalUno;
            }

            List<AreaOptimizacion> areaOptimizacionList = new List<AreaOptimizacion>();


            if ((centralTemporalUno.X != 0 || centralTemporalUno.Y != 0)
                && (centralTemporalDos.X != 0 || centralTemporalDos.Y != 0))
            {
                AreaOptimizacion areaOptimizacion = new AreaOptimizacion();
                areaOptimizacion.VerticeDerechaAbajo = new Model.ViewModels.Vector2();
                areaOptimizacion.VerticeDerechaArriba = new Model.ViewModels.Vector2();
                areaOptimizacion.VerticeIzquierdaAbajo = new Model.ViewModels.Vector2();
                areaOptimizacion.VerticeIzquierdaArriba = new Model.ViewModels.Vector2();
                areaOptimizacion.MueblesList = new List<MueblesOptmizacion>();

                areaOptimizacion.VerticeIzquierdaArriba.X = IzquierdaArriba.X;
                areaOptimizacion.VerticeIzquierdaArriba.Y = IzquierdaArriba.Y;

                areaOptimizacion.VerticeDerechaArriba.X = derechaArriba.X;
                areaOptimizacion.VerticeDerechaArriba.Y = derechaArriba.Y;

                areaOptimizacion.VerticeIzquierdaAbajo.X = izquierdaAbajo.X;
                areaOptimizacion.VerticeIzquierdaAbajo.Y = izquierdaAbajo.Y;

                areaOptimizacion.VerticeDerechaAbajo.X = derechaAbajo.X;
                areaOptimizacion.VerticeDerechaAbajo.Y = derechaAbajo.Y;

                areaOptimizacion.Area = areaOptimizacion.Ancho * areaOptimizacion.Largo;

                areaOptimizacion.Ancho = Math.Abs(areaOptimizacion.VerticeDerechaAbajo.X - areaOptimizacion.VerticeIzquierdaAbajo.X);
                areaOptimizacion.Largo = Math.Abs(areaOptimizacion.VerticeDerechaArriba.Y - areaOptimizacion.VerticeDerechaAbajo.Y);

                areaOptimizacionList.Add(areaOptimizacion);
            }
            else
            {

                AreaOptimizacion areaOptimizacion = new AreaOptimizacion();
                areaOptimizacion.VerticeDerechaAbajo = new Model.ViewModels.Vector2();
                areaOptimizacion.VerticeDerechaArriba = new Model.ViewModels.Vector2();
                areaOptimizacion.VerticeIzquierdaAbajo = new Model.ViewModels.Vector2();
                areaOptimizacion.VerticeIzquierdaArriba = new Model.ViewModels.Vector2();
                areaOptimizacion.MueblesList = new List<MueblesOptmizacion>();

                areaOptimizacion.VerticeIzquierdaArriba.X = IzquierdaArriba.X;
                areaOptimizacion.VerticeIzquierdaArriba.Y = IzquierdaArriba.Y;

                areaOptimizacion.VerticeDerechaArriba.X = derechaArriba.X;
                areaOptimizacion.VerticeDerechaArriba.Y = derechaArriba.Y;

                areaOptimizacion.VerticeIzquierdaAbajo.X = centralMin.X;
                areaOptimizacion.VerticeIzquierdaAbajo.Y = centralMin.Y;

                areaOptimizacion.VerticeDerechaAbajo.X = centralMax.X;
                areaOptimizacion.VerticeDerechaAbajo.Y = centralMax.Y;

                areaOptimizacion.Ancho = Math.Abs(areaOptimizacion.VerticeDerechaAbajo.X - areaOptimizacion.VerticeIzquierdaAbajo.X);
                areaOptimizacion.Largo = Math.Abs(areaOptimizacion.VerticeDerechaArriba.Y - areaOptimizacion.VerticeDerechaAbajo.Y);

                areaOptimizacion.Area = areaOptimizacion.Ancho * areaOptimizacion.Largo;


                areaOptimizacionList.Add(areaOptimizacion);


                areaOptimizacion = new AreaOptimizacion();
                areaOptimizacion.VerticeDerechaAbajo = new Model.ViewModels.Vector2();
                areaOptimizacion.VerticeDerechaArriba = new Model.ViewModels.Vector2();
                areaOptimizacion.VerticeIzquierdaAbajo = new Model.ViewModels.Vector2();
                areaOptimizacion.VerticeIzquierdaArriba = new Model.ViewModels.Vector2();
                areaOptimizacion.MueblesList = new List<MueblesOptmizacion>();

                areaOptimizacion.VerticeIzquierdaArriba.X = centralMin.X;
                areaOptimizacion.VerticeIzquierdaArriba.Y = centralMin.Y;

                areaOptimizacion.VerticeDerechaArriba.X = centralMax.X;
                areaOptimizacion.VerticeDerechaArriba.Y = centralMax.Y;

                areaOptimizacion.VerticeIzquierdaAbajo.X = izquierdaAbajo.X;
                areaOptimizacion.VerticeIzquierdaAbajo.Y = izquierdaAbajo.Y;

                areaOptimizacion.VerticeDerechaAbajo.X = derechaAbajo.X;
                areaOptimizacion.VerticeDerechaAbajo.Y = derechaAbajo.Y;

                areaOptimizacion.Ancho = Math.Abs(areaOptimizacion.VerticeDerechaAbajo.X - areaOptimizacion.VerticeIzquierdaAbajo.X);
                areaOptimizacion.Largo = Math.Abs(areaOptimizacion.VerticeDerechaArriba.Y - areaOptimizacion.VerticeDerechaAbajo.Y);

                areaOptimizacion.Area = Math.Abs(areaOptimizacion.Ancho * areaOptimizacion.Largo);

                areaOptimizacionList.Add(areaOptimizacion);
            }

            return areaOptimizacionList;
        }

        public double GetAnchoPasillo(LwPolyline lwPolyline, int sentido)
        {
            return 0;
        }

        /*  (2)
         
         //Particionar el espacio libre en el tamaño de la celda (incluye tamaño de la entidad y del espacio libre a su alrededor) y cargar una lista de "huecos libres"
         
        // Recorrer areaOptimizacionList

        // Calculo Cantidad de Filas en el SubArea
        // int cantFilas;
        // cantFilas = areaOptimizacion.Largo / celda.Largo;

        // Calculo Cantidad de celdas por fila en SubArea 
        // int cantCeldasFilas;
        // cantCeldasFilas = areaOptimizacion.Ancho / celda.Ancho;

        //CeldaBusiness celdaBusiness;
        
        // Asigno Primer vértice
        // public Vector2 verticeIni  { get; set; };
        // verticeIni = areaOptimizacion.VerticeIzquierdaArriba;
        
        // While i != cantFilas
        // {
        //      While j != cantCeldasFilas
        //      {
        //          Asigno Primer vértice de la celda
        //          celda.VerticeIzquierdaArriba = verticeIni;
        //
        //          celdaBusiness.CalcularDemasVertices(celda); // VER METODO MAS ABAJO
        //
        //          Agregar celda en lista de celdas...
        //          celdasList.add(celda); //TODO: Crear lista de celdas
        //
        //          verticeIni = celda.VerticeDerechaArriba;
        //      }
        //      Cambia la fila
        //      vericeIni.X = areaOptimizacion.VerticeIzquierdaArriba; // X constante para inicio de filas
        //      vericeIni.X = celda.VerticeIzquierdaArriba.Y - celda.Largo;
        //  }

        // TODO: Revisar y crear .cs para clase/ método - Esto no va ACA ---------------------------*
        // public class CeldaBusiness
        // {
        //      public CalcularDemasVertices(Celda celda)
        //      {
        //          celda.VerticeDerechaArriba.X =  celda.VerticeIzquierdaArriba.X + celda.Ancho;
        //          celda.VerticeDerechaArriba.Y =  celda.VerticeIzquierdaArriba.Y;
        //
        //          celda.VerticeIzquierdaAbajo.X = celda.VerticeIzquierdaArriba.X;
        //          celda.VerticeIzquierdaAbajo.Y = celda.VerticeIzquierdaArriba.Y - celda.Largo;
        //
        //          celda.VerticeDerechaAbajo.X = celda.VerticeIzquierdaArriba.X + celda.Ancho;
        //          celda.VerticeDerechaAbajo.Y = celda.VerticeIzquierdaArriba.Y - celda.Largo;
        //      }
        // }
        //------------------------------------------------------------------------------------------*



        //while haya muebles o espacio libre{ 
         
            //Recorro la lista de huecos y cargo los muebles

        //}
         
         */




        /*  (3)
         
         //While haya muebles en la lista de muebles{
         
            //Pega la celda a la contigua izquierda (porque si es alguna celda desp de la 1ra y se compato la anterior puede quedar despegada)

            //Adapta el tamaño de la selda al minimo segun el mueble.

            //Lo inserta en una nueva lista

            //Retorna el nuevo limite completo en el plano
         
         //}
          
         */
    }
}






/*
//List<Circle> ciruclos = initialFlat.Circles.ToList();

    Circle circulo = new Circle();
    circulo.Radius = 5;
    initialFlat.AddEntity(circulo);

    Line linea = new Line();
    Vector3 vector3 = new Vector3();
    vector3.X = 5;
    vector3.Y = 10;
    vector3.Z = 0;

    Vector3 vector4 = new Vector3();
    vector3.X = 10;
    vector3.Y = 20;
    vector3.Z = 0;
    linea.StartPoint = vector3;
    linea.EndPoint = vector4;

    initialFlat.AddEntity(linea);

//Rectangle celda = GetTamanioCelda();            Obtiene el tamaño que se usara para particionar el plano.

//return initialFlat;

*/
