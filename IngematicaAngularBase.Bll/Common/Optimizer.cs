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
        

        //Agregar consideracion de la escala del plano. Los muebles los tomamos en metros en el momento del alta. Al final.
        public DxfDocument Generate()
        {

            // validar cantidad de personas por metro cuadrado. Si hay de mas => lanzo excepcion.
            //Considero que la cantida de muebles solicitados es para el total de regiones que esten en el dxf

            DxfDocument finalFlat = new DxfDocument();
            List<LwPolylineVertex> vertices = new List<LwPolylineVertex>();
            decimal anchoPasillos = 0; 
            Celda celda = new Celda();
            List<Mueble> muebleListTemp;
            bool hayEspacio;//Se usa dsp de la compactacion

            foreach(var sentido in Enum.GetValues(typeof(SentidoPasillosEnum)))
            {
                muebleListTemp = new List<Mueble>(muebleList).OrderBy(x => x.OrdenDePrioridad).ToList();

                foreach (LwPolyline lwPolyline in initialFlat.LwPolylines)
                {
                    anchoPasillos = GetAnchoPasillo(lwPolyline, (int)sentido); //TODO
                    vertices = lwPolyline.Vertexes;
                    hayEspacio = true;
                    List<AreaOptimizacion> areaOptmizacion = GetSubAreas(vertices);

                    while (muebleListTemp.Count > 0 && hayEspacio)
                    {
                        celda = GetTamañoMaximoCelda(muebleList, (int)sentido);
                        //ubicarMuebles(areaOptmizacion, muebleList, sentido, anchoPasillo) -> Eliminarlos de muebleListTemp. Ubica muebles x fila + pasillo
                        //hayEspacio = Compactar(areaOptmizacion, sentido) -> Compacta la lista de zonasOcupadas y retorna si queda lugar libre
                    }
                }
            }

            return null;
        }

        //Metodo que retorna el tamaño de la celda.
        //Recibe el sentido en que se ponen los muebles, porque si algun mueble posee radio mayor, 
        //se considera en el sentido de recorrido del plano.
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
                        celda.Largo = (decimal)RadioMayor;
                    else if(Largo != null)
                        celda.Largo = (decimal)Largo;
                    else
                        celda.Largo = (decimal)RadioMayor;

                    if (RadioMenor != null && Ancho != null && RadioMenor > Ancho)
                        celda.Ancho = (decimal)RadioMenor;
                    else if (Ancho != null)
                        celda.Ancho = (decimal)Ancho;
                    else
                        celda.Ancho = (decimal)RadioMenor;

                    break;

                case 2:

                    if (RadioMayor != null && Ancho != null && RadioMayor > Ancho)
                        celda.Ancho = (decimal)RadioMayor;
                    else if (Ancho != null)
                        celda.Ancho = (decimal)Ancho;
                    else
                        celda.Ancho = (decimal)RadioMayor;

                    if (RadioMenor != null && Largo != null && RadioMenor > Largo)
                        celda.Largo = (decimal)RadioMenor;
                    else if (Largo != null)
                        celda.Largo = (decimal)Largo;
                    else
                        celda.Largo = (decimal)RadioMenor;

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

            if (centralTemporalUno != null && centralTemporalDos != null && centralTemporalUno.X > centralTemporalDos.X)
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


            if (centralTemporalUno == null && centralTemporalDos == null)
            {
                AreaOptimizacion areaOptimizacion = new AreaOptimizacion();
                areaOptimizacion.MueblesList = new List<MueblesOptmizacion>();

                areaOptimizacion.VerticeIzquierdaArriba.X = IzquierdaArriba.X;
                areaOptimizacion.VerticeIzquierdaArriba.Y = IzquierdaArriba.Y;

                areaOptimizacion.VerticeDerechaArriba.X = derechaArriba.X;
                areaOptimizacion.VerticeDerechaArriba.Y = derechaArriba.Y;

                areaOptimizacion.VerticeIzquierdaAbajo.X = izquierdaAbajo.X;
                areaOptimizacion.VerticeIzquierdaAbajo.Y = izquierdaAbajo.Y;

                areaOptimizacion.VerticeDerechaAbajo.X = derechaAbajo.X;
                areaOptimizacion.VerticeDerechaAbajo.Y = derechaAbajo.Y;

                areaOptimizacion.Area = Math.Abs((areaOptimizacion.VerticeDerechaAbajo.X - areaOptimizacion.VerticeIzquierdaAbajo.X)
                                                   * (areaOptimizacion.VerticeDerechaArriba.Y - areaOptimizacion.VerticeDerechaAbajo.Y));

                // areaOptimizacion.Ancho = Math.Abs(areaOptimizacion.VerticeDerechaAbajo.X - areaOptimizacion.VerticeIzquierdaAbajo.X);
                // areaOptimizacion.Largo = Math.Abs(areaOptimizacion.VerticeDerechaArriba.Y - areaOptimizacion.VerticeDerechaAbajo.Y);

                areaOptimizacionList.Add(areaOptimizacion);
            }
            else
            {

                AreaOptimizacion areaOptimizacion = new AreaOptimizacion();
                areaOptimizacion.MueblesList = new List<MueblesOptmizacion>();

                areaOptimizacion.VerticeIzquierdaArriba.X = IzquierdaArriba.X;
                areaOptimizacion.VerticeIzquierdaArriba.Y = IzquierdaArriba.Y;

                areaOptimizacion.VerticeDerechaArriba.X = derechaArriba.X;
                areaOptimizacion.VerticeDerechaArriba.Y = derechaArriba.Y;

                areaOptimizacion.VerticeIzquierdaAbajo.X = centralMin.X;
                areaOptimizacion.VerticeIzquierdaAbajo.Y = centralMin.Y;

                areaOptimizacion.VerticeDerechaAbajo.X = centralMax.X;
                areaOptimizacion.VerticeDerechaAbajo.Y = centralMax.Y;

                // TODO: Revisar y descomentar (Descomentar también los atributos en la def. de areaOptimizacion)
                // areaOptimizacion.Ancho = Math.Abs(areaOptimizacion.VerticeDerechaAbajo.X - areaOptimizacion.VerticeIzquierdaAbajo.X);
                // areaOptimizacion.Largo = Math.Abs(areaOptimizacion.VerticeDerechaArriba.Y - areaOptimizacion.VerticeDerechaAbajo.Y);

                areaOptimizacion.Area = Math.Abs((areaOptimizacion.VerticeDerechaAbajo.X - areaOptimizacion.VerticeIzquierdaAbajo.X)
                                   * (areaOptimizacion.VerticeDerechaArriba.Y - areaOptimizacion.VerticeDerechaAbajo.Y));

                // TODO: Si esta bien Ancho y Largo, reemplazar cálculo de Area :
                // areaOptimizacion.Area = Math.Abs(areaOptimizacion.Ancho * areaOptimizacion.Largo);

                areaOptimizacionList.Add(areaOptimizacion);


                areaOptimizacion = new AreaOptimizacion();
                areaOptimizacion.MueblesList = new List<MueblesOptmizacion>();

                areaOptimizacion.VerticeIzquierdaArriba.X = centralMin.X;
                areaOptimizacion.VerticeIzquierdaArriba.Y = centralMin.Y;

                areaOptimizacion.VerticeDerechaArriba.X = centralMax.X;
                areaOptimizacion.VerticeDerechaArriba.Y = centralMax.Y;

                areaOptimizacion.VerticeIzquierdaAbajo.X = izquierdaAbajo.X;
                areaOptimizacion.VerticeIzquierdaAbajo.Y = izquierdaAbajo.Y;

                areaOptimizacion.VerticeDerechaAbajo.X = derechaAbajo.X;
                areaOptimizacion.VerticeDerechaAbajo.Y = derechaAbajo.Y;

                // TODO: Revisar y descomentar
                // areaOptimizacion.Ancho = Math.Abs(areaOptimizacion.VerticeDerechaAbajo.X - areaOptimizacion.VerticeIzquierdaAbajo.X);
                // areaOptimizacion.Largo = Math.Abs(areaOptimizacion.VerticeDerechaArriba.Y - areaOptimizacion.VerticeDerechaAbajo.Y);

                areaOptimizacion.Area = Math.Abs((areaOptimizacion.VerticeDerechaAbajo.X - areaOptimizacion.VerticeIzquierdaAbajo.X)
                                   * (areaOptimizacion.VerticeDerechaArriba.Y - areaOptimizacion.VerticeDerechaAbajo.Y));

                // TODO: Si esta bien Ancho y Largo, reemplazar cálculo de Area :
                // areaOptimizacion.Area = Math.Abs(areaOptimizacion.Ancho * areaOptimizacion.Largo);

                areaOptimizacionList.Add(areaOptimizacion);
            }

            return areaOptimizacionList;
        }

        public decimal GetAnchoPasillo(LwPolyline lwPolyline, int sentido)
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
