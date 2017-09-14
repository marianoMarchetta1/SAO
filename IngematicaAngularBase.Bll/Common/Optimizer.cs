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
        public int? cantidadPersonas;

        public Optimizer(DxfDocument dxfDocument, bool optimizarCostoParam, decimal costoMaximoParam, List<OptimizacionMueble> muebleCantidadListParam, List<Mueble> muebleListParam, string escalaParam, int? cantidadPersonasParam)
        {
            this.initialFlat = dxfDocument;
            this.optimizarCosto = optimizarCostoParam;
            this.costoMaximo = costoMaximoParam;
            this.muebleCantidadList = muebleCantidadListParam;
            this.muebleList = muebleListParam;
            this.escala = escalaParam;
            this.cantidadPersonas = cantidadPersonasParam;
        }


        #region
        //Agregar consideracion de la escala del plano. Los muebles los tomamos en metros en el momento del alta. Al final.
        #endregion
        public List<DxfDocument> Generate()
        {
            #region
            // validar cantidad de personas por metro cuadrado. Si hay de mas => lanzo excepcion.
            //Considero que la cantida de muebles solicitados es para el total de regiones que esten en el dxf
            #endregion

            List<DxfDocument> finalFlats = new List<DxfDocument>();
            DxfDocument finalFlat = new DxfDocument();
            List<LwPolylineVertex> vertices = new List<LwPolylineVertex>();
            double anchoPasillos = 0; 
            Celda celda = new Celda();
            List<Mueble> muebleListTemp;
            bool hayEspacio;//Se usa dsp de la compactacion
            int index;
            int factorEscala;

            factorEscala = GetEscala();
            #region
            //Cada mueble se tiene que repetir la cantidad de veces que este en muebleCantidadList
            #endregion
            muebleList = ReplicacarMuebles(factorEscala);

            foreach(var sentido in Enum.GetValues(typeof(SentidoPasillosEnum)))
            {               
                muebleListTemp = new List<Mueble>(muebleList).OrderBy(x => x.OrdenDePrioridad).ToList();

                List<List<AreaOptimizacion>> plano = new List<List<AreaOptimizacion>>();

                foreach (LwPolyline lwPolyline in initialFlat.LwPolylines)
                {
                    anchoPasillos = GetAnchoPasillo(lwPolyline, (int)sentido, factorEscala);
                    vertices = lwPolyline.Vertexes;
                    hayEspacio = true;
                    List<AreaOptimizacion> areaOptmizacion = GetSubAreas(vertices);

                    while (muebleListTemp.Count > 0 && hayEspacio)
                    {
                        celda = GetTamañoMaximoCelda(muebleListTemp, (int)sentido);
                        index = UbicarMuebles(areaOptmizacion, muebleListTemp, (int)sentido, anchoPasillos, celda);

                        if (index == -1)
                        {
                            //hayEspacio = Compactar(areaOptmizacion, sentido) -> Compacta la lista de zonasOcupadas y retorna si queda lugar libre en algun subarea
                            hayEspacio = false;

                            if (!hayEspacio)
                                break;
                        }                                                                     
                    }

                    #region
                    //Inserto cada polyline o area luego de insertar todos los muebles que pueda, las compactaciones, etc.
                    #endregion
                    plano.Add(areaOptmizacion);
                }

                finalFlat = GrabarPlano(plano);
                finalFlats.Add(finalFlat);
                finalFlat = new DxfDocument();
            }

            return finalFlats;
        }

        public DxfDocument GrabarPlano(List<List<AreaOptimizacion>> areaOptimizacion)
        {            
            //string filename = "C:\\Temp\\Test_4.dxf"; // LINEA PARA TESTING - BORRAR

            MuebleBusiness mb = new MuebleBusiness();

            foreach (List<AreaOptimizacion> areaList in areaOptimizacion)
            {
                foreach (AreaOptimizacion area in areaList)
                {
                    foreach (MueblesOptmizacion mueble in area.MueblesList)
                    {
                        mueble.VerticeIzquierdaArriba.X += System.Convert.ToDouble(mueble.Mueble.DistanciaParedes + mueble.Mueble.DistanciaProximoMueble);
                        mueble.VerticeIzquierdaArriba.Y -= System.Convert.ToDouble(mueble.Mueble.DistanciaParedes + mueble.Mueble.DistanciaProximoMueble);

                        mueble.VerticeDerechaArriba.X -= System.Convert.ToDouble(mueble.Mueble.DistanciaParedes + mueble.Mueble.DistanciaProximoMueble);
                        mueble.VerticeDerechaArriba.Y -= System.Convert.ToDouble(mueble.Mueble.DistanciaParedes + mueble.Mueble.DistanciaProximoMueble);

                        mueble.VerticeIzquierdaAbajo.X += System.Convert.ToDouble(mueble.Mueble.DistanciaParedes + mueble.Mueble.DistanciaProximoMueble);
                        mueble.VerticeIzquierdaAbajo.Y += System.Convert.ToDouble(mueble.Mueble.DistanciaParedes + mueble.Mueble.DistanciaProximoMueble);

                        mueble.VerticeDerechaAbajo.X -= System.Convert.ToDouble(mueble.Mueble.DistanciaParedes + mueble.Mueble.DistanciaProximoMueble);
                        mueble.VerticeDerechaAbajo.Y += System.Convert.ToDouble(mueble.Mueble.DistanciaParedes + mueble.Mueble.DistanciaProximoMueble);
                    }
                }
            }

            DxfDocument dxfFinal = new DxfDocument();

            foreach (LwPolyline lwpl in initialFlat.LwPolylines)
            {
                LwPolyline lwplClon = (LwPolyline)lwpl.Clone(); 
                dxfFinal.AddEntity(lwplClon);
            }

            foreach (List<AreaOptimizacion> areaList in areaOptimizacion)
            {
                foreach (AreaOptimizacion area in areaList)
                {
                    foreach (MueblesOptmizacion mueble in area.MueblesList)
                    {
                        if (!mueble.Mueble.PoseeRadio)
                        {
                            List<LwPolylineVertex> lpVertex = new List<LwPolylineVertex>();
                            lpVertex.Add(mb.ConvertVertex(mueble.VerticeIzquierdaAbajo));
                            lpVertex.Add(mb.ConvertVertex(mueble.VerticeIzquierdaArriba));
                            lpVertex.Add(mb.ConvertVertex(mueble.VerticeDerechaArriba));
                            lpVertex.Add(mb.ConvertVertex(mueble.VerticeDerechaAbajo));
                            dxfFinal.AddEntity(new LwPolyline(lpVertex, true));

                        }
                        else if(mueble.Mueble.RadioMayor == null || mueble.Mueble.RadioMenor == null)
                        {
                            Circle circulo = new Circle();
                            circulo.Radius = mueble.Mueble.RadioMayor != null ? System.Convert.ToDouble(mueble.Mueble.RadioMayor) : System.Convert.ToDouble(mueble.Mueble.RadioMenor);
                            circulo.Center = GetCentro(mueble);
                            dxfFinal.AddEntity(circulo);
                        }
                        else
                        {
                            Ellipse elipse = new Ellipse();
                            elipse.MajorAxis = System.Convert.ToDouble(mueble.Mueble.RadioMayor);
                            elipse.MinorAxis = System.Convert.ToDouble(mueble.Mueble.RadioMenor);
                            elipse.Center = GetCentro(mueble);
                            dxfFinal.AddEntity(elipse);
                        }
                    }
                }
            }
            //dxfFinal.Save(filename); // LINEA PARA TESTING - BORRAR 
            return dxfFinal;
        }

        public Vector3 GetCentro(MueblesOptmizacion muebleOptimizacion)
        {
            Vector3 centro = new Vector3();

            centro.X = (muebleOptimizacion.VerticeDerechaArriba.X - muebleOptimizacion.VerticeIzquierdaArriba.X) / 2;
            centro.Y = (muebleOptimizacion.VerticeIzquierdaArriba.Y - muebleOptimizacion.VerticeIzquierdaAbajo.Y) / 2;
            centro.Z = 0;

            return centro;
        }

        public int GetEscala()
        {
            string result = "";

            for(int i = 0; i < escala.Length; i++)
            {
                if (escala.ElementAt(i) != ':')
                    result += escala.ElementAt(i);
                else
                    break;
            }

            return Int32.Parse(result);
        }


        public List<Mueble> ReplicacarMuebles(int factorEscala)
        {
            List<Mueble> mueblesReplicados = new List<Mueble>();
            int cantidad = 0;

            foreach(Mueble mueble in muebleList)
            {
                cantidad = muebleCantidadList.Where(x => x.IdMueble == mueble.IdMueble).Select(x => x.Cantidad).First();

                for(int i = 0; i < cantidad; i++)
                {
                    MuebleBusiness mb = new MuebleBusiness();
                    Mueble muebleCopy =  mb.Clone(mueble);
                    muebleCopy.Ancho                  = muebleCopy.Ancho * factorEscala;
                    muebleCopy.DistanciaParedes       = muebleCopy.DistanciaParedes * factorEscala;
                    muebleCopy.DistanciaProximoMueble = muebleCopy.DistanciaProximoMueble * factorEscala;
                    muebleCopy.Largo                  = muebleCopy.Largo * factorEscala;
                    muebleCopy.RadioMayor             = muebleCopy.RadioMayor * factorEscala;
                    muebleCopy.RadioMenor             = muebleCopy.RadioMenor * factorEscala;
                    mueblesReplicados.Add(muebleCopy);
                }
            }

            return mueblesReplicados;
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

            int index = 0;
            
            if(sentido == (int)SentidoPasillosEnum.Largo)
            {
                index = GetSubAreaConEspacioLargo(areaOptimizacion, celda);

                if (index == -1)
                    return index;

                #region
                //obtengo el ultimo mueble. Verifico si hay lugar en esa fila, sino uso la siguiente si es que entra
                #endregion
                UbicarMueblesEnLargo(areaOptimizacion[index], muebleList, sentido, anchoPasillos, celda);

            } else if (sentido == (int)SentidoPasillosEnum.Ancho)
            {
                index = GetSubAreaConEspacioAncho(areaOptimizacion, celda);

                if (index == -1)
                    return index;

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

            if (areaOptimizacion.MueblesList.Count == 0)
            {
                #region
                // Area Vacía
                #endregion
                xMax            = areaOptimizacion.VerticeIzquierdaArriba.X + celda.Ancho;
                izquierdaAbajo  = areaOptimizacion.VerticeIzquierdaArriba;
                derechaAbajo.X  = areaOptimizacion.VerticeIzquierdaArriba.X + celda.Ancho;
                derechaAbajo.Y  = areaOptimizacion.VerticeIzquierdaArriba.Y;
            }
            else
            { 
                xMax           = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();
                derechaAbajo   = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo).Where(x => x.X == xMax).Min();
                izquierdaAbajo = areaOptimizacion.MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeIzquierdaAbajo).First();
            }

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

                if (areaOptimizacion.MueblesList.Count > 0)
                    xMax = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();

                if (celdaList.Count != 0)
                {
                    InsertarPasilloEnLargo(areaOptimizacion, anchoPasillos, xMax);
                }  
            }

            #region
            //si no valido esto pueden quedar 2 pasillos contiguos
            #endregion
            if (muebleList.Count > 0 && areaOptimizacion.MueblesList.Count != 0)
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

                    if (celdaList.Count != 0)
                    {
                        InsertarPasilloEnLargo(areaOptimizacion, anchoPasillos, xMax);
                    }

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

                pasillo.Ancho = anchoPasillos;
                pasillo.Largo = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
                pasillo.Area = pasillo.Ancho * pasillo.Largo;

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
            List<MueblesOptmizacion> celdaList = new List<MueblesOptmizacion>();


            if (xMax + celda.Ancho <= xMaxArea)
            {

                MueblesOptmizacion celdaMueble = new MueblesOptmizacion();

                Model.ViewModels.Vector2 VerticeIzquierdaArriba = new Model.ViewModels.Vector2();
                VerticeIzquierdaArriba.Y = areaOptimizacion.VerticeIzquierdaArriba.Y;
                VerticeIzquierdaArriba.X = xMax;
                celdaMueble.VerticeIzquierdaArriba = VerticeIzquierdaArriba;

                Model.ViewModels.Vector2 VerticeDerechaArriba = new Model.ViewModels.Vector2();
                VerticeDerechaArriba.Y = areaOptimizacion.VerticeDerechaArriba.Y;
                VerticeDerechaArriba.X = xMax + celda.Ancho;
                celdaMueble.VerticeDerechaArriba = VerticeDerechaArriba;

                Model.ViewModels.Vector2 VerticeIzquierdaAbajo = new Model.ViewModels.Vector2();
                VerticeIzquierdaAbajo.Y = VerticeIzquierdaArriba.Y - celda.Largo;
                VerticeIzquierdaAbajo.X = xMax;
                celdaMueble.VerticeIzquierdaAbajo = VerticeIzquierdaAbajo;

                Model.ViewModels.Vector2 VerticeDerechaAbajo = new Model.ViewModels.Vector2();
                VerticeDerechaAbajo.Y = VerticeDerechaArriba.Y - celda.Largo;
                VerticeDerechaAbajo.X = xMax + celda.Ancho;
                celdaMueble.VerticeDerechaAbajo = VerticeDerechaAbajo;

                celdaMueble.Mueble = new Mueble();
                celdaMueble.Largo = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                celdaMueble.Ancho = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
                celdaMueble.Area = celdaMueble.Largo * celdaMueble.Ancho;

                celdaList.Add(celdaMueble);


                while (celdaMueble.VerticeIzquierdaAbajo.Y - celda.Largo >= areaOptimizacion.VerticeIzquierdaAbajo.Y)
                {
                    celdaMueble = new MueblesOptmizacion();

                    VerticeIzquierdaArriba = celdaList.Last().VerticeIzquierdaAbajo;
                    celdaMueble.VerticeIzquierdaArriba = VerticeIzquierdaArriba;

                    VerticeDerechaArriba = celdaList.Last().VerticeDerechaAbajo;
                    celdaMueble.VerticeDerechaArriba = VerticeDerechaArriba;

                    VerticeIzquierdaAbajo = new Model.ViewModels.Vector2();
                    VerticeIzquierdaAbajo.Y = VerticeIzquierdaArriba.Y - celda.Largo;
                    VerticeIzquierdaAbajo.X = VerticeIzquierdaArriba.X;
                    celdaMueble.VerticeIzquierdaAbajo = VerticeIzquierdaAbajo;

                    VerticeDerechaAbajo = new Model.ViewModels.Vector2();
                    VerticeDerechaAbajo.Y = VerticeDerechaArriba.Y - celda.Largo;
                    VerticeDerechaAbajo.X = VerticeDerechaArriba.X;
                    celdaMueble.VerticeDerechaAbajo = VerticeDerechaAbajo;

                    celdaMueble.Mueble = new Mueble();
                    celdaMueble.Largo = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                    celdaMueble.Ancho = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
                    celdaMueble.Area = celdaMueble.Largo * celdaMueble.Ancho;

                    celdaList.Add(celdaMueble);      
                }

            }

            return celdaList;
        }

        #region
        //Recibe el xMaxArea, este caso se da solo despues de una compactacion => el tamaño de la celda puede ser distinto al que se venia trabajando
        //en la iteracion anterior => tengo que validar no excederme de las dimenciones del plano.
        #endregion
        public List<MueblesOptmizacion> GetCeldaList(double xMaxArea, Celda celda, Model.ViewModels.Vector2 izquierdaAbajo, double limiteInferior)
        {
            List<MueblesOptmizacion> celdaList = new List<MueblesOptmizacion>();
            if (izquierdaAbajo.X + celda.Ancho <= xMaxArea)
            {
                while (izquierdaAbajo.Y - celda.Largo >= limiteInferior)
                {
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
            }
            return celdaList;
        }

        public void UbicarMueblesEnAncho(AreaOptimizacion areaOptimizacion, List<Mueble> muebleList, int sentido, double anchoPasillos, Celda celda)
        {
            List<MueblesOptmizacion> celdaList = new List<MueblesOptmizacion>();

            Model.ViewModels.Vector2 izquierdaAbajo = new Model.ViewModels.Vector2();
            Model.ViewModels.Vector2 izquierdaArriba = new Model.ViewModels.Vector2();
            double yMin;
            double xMaxArea;

            xMaxArea = areaOptimizacion.VerticeDerechaAbajo.X;

            if (areaOptimizacion.MueblesList.Count == 0)
            {
                #region
                // Area Vacía
                #endregion
                yMin = areaOptimizacion.VerticeIzquierdaArriba.Y;

                izquierdaArriba.X = areaOptimizacion.VerticeIzquierdaArriba.X;
                izquierdaArriba.Y = areaOptimizacion.VerticeIzquierdaArriba.Y;

                izquierdaAbajo.X = areaOptimizacion.VerticeIzquierdaArriba.X;
                izquierdaAbajo.Y = izquierdaArriba.Y - celda.Largo;
            }
            else
            {
                yMin = areaOptimizacion.MueblesList.Select(y => y.VerticeIzquierdaAbajo.Y).Min();
                izquierdaAbajo = areaOptimizacion.MueblesList.Select(x => x.VerticeIzquierdaAbajo).Where(y => y.Y == yMin).Max();
                izquierdaArriba = areaOptimizacion.MueblesList.Where(x => x.VerticeIzquierdaAbajo == izquierdaAbajo).Select(x => x.VerticeIzquierdaArriba).First();

                izquierdaArriba.X = izquierdaArriba.X;
                izquierdaArriba.Y = izquierdaArriba.Y;

                izquierdaAbajo.X = izquierdaArriba.X;
                izquierdaAbajo.Y = izquierdaArriba.Y - celda.Largo;
            }

            #region
            //Completo la fila incompleta
            #endregion
            if (izquierdaAbajo.X + celda.Ancho < areaOptimizacion.VerticeDerechaAbajo.X && 
                izquierdaAbajo.Y > areaOptimizacion.VerticeIzquierdaAbajo.Y)
            {

                celdaList = GetCeldaListHorizontal(xMaxArea, celda, izquierdaArriba);

                #region
                //verificar que muebleList y areaOptimizacion se esten pasando por referencia.
                #endregion
                foreach (MueblesOptmizacion celdaTemp in celdaList)
                    IncertarMuebleCelda(celdaTemp, muebleList, areaOptimizacion);

                if (areaOptimizacion.MueblesList.Count > 0)
                    yMin = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.Y).Min();

                if (celdaList.Count > 0) //Sino inserta dos a lo ultimo
                    InsertarPasilloEnAncho(areaOptimizacion, anchoPasillos, yMin);
            }

            #region
            //si no valido esto pueden quedar 2 pasillos contiguos
            #endregion
            if (muebleList.Count > 0 && areaOptimizacion.MueblesList.Count != 0 )
            {
                do
                {

                    yMin = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.Y).Min();

                    #region
                    //Genera una nueva fila de celdas
                    //usa xMax porque en caso de que quede formato cerrucho x compactacion, se desprecia una pequeña parte del plano
                    #endregion
                    celdaList = GetCeldaListHorizontal(celda, yMin, areaOptimizacion);

                    #region
                    //tanto GetCeldaList como InsertarPasillo validan no pasarse del limite del subarea
                    #endregion
                    foreach (MueblesOptmizacion celdaTemp in celdaList)
                        IncertarMuebleCelda(celdaTemp, muebleList, areaOptimizacion);

                    yMin = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.Y).Min();

                    if (celdaList.Count > 0) //Sino inserta dos a lo ultimo
                        InsertarPasilloEnAncho(areaOptimizacion, anchoPasillos, yMin);
                        
                } while (muebleList.Count > 0 && celdaList.Count > 0);
                #region
                //con celdaList > 0 valido que se haya incertado algo en la iteracion anterior
                #endregion
            }
        }

        public List<MueblesOptmizacion> GetCeldaListHorizontal(Celda celda, double yMin, AreaOptimizacion areaOptimizacion)
        {
            List<MueblesOptmizacion> celdaList = new List<MueblesOptmizacion>();

            if (yMin - celda.Largo >= areaOptimizacion.VerticeDerechaAbajo.Y)
            {
                MueblesOptmizacion celdaMueble = new MueblesOptmizacion();

                Model.ViewModels.Vector2 VerticeIzquierdaArriba = new Model.ViewModels.Vector2();
                VerticeIzquierdaArriba.Y = yMin;
                VerticeIzquierdaArriba.X = areaOptimizacion.VerticeIzquierdaAbajo.X;
                celdaMueble.VerticeIzquierdaArriba = VerticeIzquierdaArriba;

                Model.ViewModels.Vector2 VerticeDerechaArriba = new Model.ViewModels.Vector2();
                VerticeDerechaArriba.Y = VerticeIzquierdaArriba.Y;
                VerticeDerechaArriba.X = VerticeIzquierdaArriba.X + celda.Ancho;
                celdaMueble.VerticeDerechaArriba = VerticeDerechaArriba;

                Model.ViewModels.Vector2 VerticeIzquierdaAbajo = new Model.ViewModels.Vector2();
                VerticeIzquierdaAbajo.Y = VerticeIzquierdaArriba.Y - celda.Largo;
                VerticeIzquierdaAbajo.X = VerticeIzquierdaArriba.X;
                celdaMueble.VerticeIzquierdaAbajo = VerticeIzquierdaAbajo;

                Model.ViewModels.Vector2 VerticeDerechaAbajo = new Model.ViewModels.Vector2();
                VerticeDerechaAbajo.Y = VerticeDerechaArriba.Y - celda.Largo;
                VerticeDerechaAbajo.X = VerticeDerechaArriba.X;
                celdaMueble.VerticeDerechaAbajo = VerticeDerechaAbajo;

                celdaMueble.Mueble = new Mueble();
                celdaMueble.Largo = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                celdaMueble.Ancho = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
                celdaMueble.Area = celdaMueble.Largo * celdaMueble.Ancho;

                celdaList.Add(celdaMueble);


                while (celdaMueble.VerticeDerechaAbajo.X + celda.Ancho <= areaOptimizacion.VerticeDerechaAbajo.X)
                {
                    celdaMueble = new MueblesOptmizacion();

                    VerticeIzquierdaArriba = celdaList.Last().VerticeDerechaArriba;
                    celdaMueble.VerticeIzquierdaArriba = VerticeIzquierdaArriba;

                    VerticeIzquierdaAbajo = celdaList.Last().VerticeDerechaAbajo;
                    celdaMueble.VerticeIzquierdaAbajo = VerticeIzquierdaAbajo;

                    VerticeDerechaArriba = new Model.ViewModels.Vector2();
                    VerticeDerechaArriba.Y = VerticeIzquierdaArriba.Y;
                    VerticeDerechaArriba.X = VerticeIzquierdaArriba.X +celda.Ancho;
                    celdaMueble.VerticeDerechaArriba = VerticeDerechaArriba;

                    VerticeDerechaAbajo = new Model.ViewModels.Vector2();
                    VerticeDerechaAbajo.Y = VerticeIzquierdaAbajo.Y;
                    VerticeDerechaAbajo.X = VerticeIzquierdaAbajo.X + celda.Ancho;
                    celdaMueble.VerticeDerechaAbajo = VerticeDerechaAbajo;

                    celdaMueble.Mueble = new Mueble();
                    celdaMueble.Largo = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                    celdaMueble.Ancho = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
                    celdaMueble.Area = celdaMueble.Largo * celdaMueble.Ancho;

                    celdaList.Add(celdaMueble);
                }

            }

            return celdaList;
        }

        public void InsertarPasilloEnAncho(AreaOptimizacion areaOptimizacion, double anchoPasillos, double yMin)
        {
            if (yMin - anchoPasillos >= areaOptimizacion.VerticeDerechaAbajo.Y)
            {
                MueblesOptmizacion pasillo = new MueblesOptmizacion();
                pasillo.Mueble = new Mueble();

                Model.ViewModels.Vector2 VerticeIzquierdaArriba = new Model.ViewModels.Vector2();
                VerticeIzquierdaArriba.Y = yMin;
                VerticeIzquierdaArriba.X = areaOptimizacion.VerticeIzquierdaArriba.X;
                pasillo.VerticeIzquierdaArriba = VerticeIzquierdaArriba;

                Model.ViewModels.Vector2 VerticeIzquierdaAbajo = new Model.ViewModels.Vector2();
                VerticeIzquierdaAbajo.Y = yMin - anchoPasillos;
                VerticeIzquierdaAbajo.X = areaOptimizacion.VerticeIzquierdaArriba.X;
                pasillo.VerticeIzquierdaAbajo = VerticeIzquierdaAbajo;

                Model.ViewModels.Vector2 VerticeDerechaArriba = new Model.ViewModels.Vector2();
                VerticeDerechaArriba.Y = VerticeIzquierdaArriba.Y;
                VerticeDerechaArriba.X = areaOptimizacion.VerticeDerechaArriba.X;
                pasillo.VerticeDerechaArriba = VerticeDerechaArriba;

                Model.ViewModels.Vector2 VerticeDerechaAbajo = new Model.ViewModels.Vector2();
                VerticeDerechaAbajo.Y = VerticeIzquierdaAbajo.Y;
                VerticeDerechaAbajo.X = areaOptimizacion.VerticeDerechaArriba.X;
                pasillo.VerticeDerechaAbajo = VerticeDerechaAbajo;

                areaOptimizacion.MueblesList.Add(pasillo);
            }
        }

        public List<MueblesOptmizacion> GetCeldaListHorizontal(double xMaxArea, Celda celda, Model.ViewModels.Vector2 izquierdaArriba)
        {
            List<MueblesOptmizacion> celdaList = new List<MueblesOptmizacion>();

            while (izquierdaArriba.X + celda.Ancho <= xMaxArea)
            {

                MueblesOptmizacion celdaMueble = new MueblesOptmizacion();
                celdaMueble.VerticeIzquierdaArriba = izquierdaArriba;

                Model.ViewModels.Vector2 VerticeIzquierdaAbajo = new Model.ViewModels.Vector2();
                VerticeIzquierdaAbajo.Y = celdaMueble.VerticeIzquierdaArriba.Y - celda.Largo;
                VerticeIzquierdaAbajo.X = celdaMueble.VerticeIzquierdaArriba.X;
                celdaMueble.VerticeIzquierdaAbajo = VerticeIzquierdaAbajo;

                Model.ViewModels.Vector2 VerticeDerechaArriba = new Model.ViewModels.Vector2();
                VerticeDerechaArriba.Y = celdaMueble.VerticeIzquierdaArriba.Y;
                VerticeDerechaArriba.X = celdaMueble.VerticeIzquierdaArriba.X + celda.Ancho;
                celdaMueble.VerticeDerechaArriba = VerticeDerechaArriba;

                Model.ViewModels.Vector2 VerticeDerechaAbajo = new Model.ViewModels.Vector2();
                VerticeDerechaAbajo.Y = celdaMueble.VerticeDerechaArriba.Y - celda.Largo;
                VerticeDerechaAbajo.X = celdaMueble.VerticeDerechaArriba.X;
                celdaMueble.VerticeDerechaAbajo = VerticeDerechaAbajo;

                celdaMueble.Mueble = new Mueble();
                celdaMueble.Largo = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                celdaMueble.Ancho = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
                celdaMueble.Area = celdaMueble.Largo * celdaMueble.Ancho;

                celdaList.Add(celdaMueble);

                izquierdaArriba = celdaMueble.VerticeDerechaArriba;
            }
            return celdaList;
        }

        #region
        //Obtengo el mueble que tenga las mayores coordenadas y verifico si coinciden con el ultimo vertice
        //del subarea y que el area de la celda sea menor al lugar vacion.
        //Seguro va a haber alguna porque ya retorno en la compactacion si hay queda espacio en algun area.
        #endregion
        public int GetSubAreaConEspacioLargo(List<AreaOptimizacion> areaOptimizacion, Celda celda)
        {
            MueblesOptmizacion muebleOptimizacion    = new MueblesOptmizacion();
            Model.ViewModels.Vector2 izquierdaArriba = new Model.ViewModels.Vector2();
            Model.ViewModels.Vector2 derechaArriba   = new Model.ViewModels.Vector2();
            Model.ViewModels.Vector2 izquierdaAbajo  = new Model.ViewModels.Vector2();
            Model.ViewModels.Vector2 derechaAbajo    = new Model.ViewModels.Vector2();
            double xMax;
            double yMin;

            for (int i = 0; i < areaOptimizacion.Count; i++)
            {
                if (areaOptimizacion[i].MueblesList.Any())
                {
                    xMax = areaOptimizacion[i].MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();

                    List<Model.ViewModels.Vector2> listVertices = areaOptimizacion[i].MueblesList.Select(x => x.VerticeDerechaAbajo).Where(x => x.X == xMax).ToList();
                    yMin = listVertices.Select(y => y.Y).Min();
                    
                    // Vertice derechaAbajo del ultimo mueble agregado
                    derechaAbajo    = listVertices.Where(y => y.Y == yMin).Select(y => y).First();
                    derechaArriba   = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeDerechaArriba).First();
                    izquierdaAbajo  = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeIzquierdaAbajo).First();
                    izquierdaArriba = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeIzquierdaArriba).First();
                    
                    //Si coinciden los vertices => el area esta llena => paso a la siguiente area.
                    if (derechaAbajo == areaOptimizacion[i].VerticeDerechaAbajo)
                        continue;

                    //if ((areaOptimizacion[i].Area - areaOptimizacion[i].MueblesList.Sum(x => x.Area)) > (celda.Ancho * celda.Largo))
                    double areaRestoFila = (Math.Abs(derechaAbajo.X - izquierdaAbajo.X)) * (Math.Abs(derechaAbajo.Y - areaOptimizacion[i].VerticeDerechaAbajo.Y));
                    bool   ultimaFila    = (Math.Abs(areaOptimizacion[i].VerticeDerechaAbajo.X - xMax)) < celda.Ancho;
                    
                    if ( (areaRestoFila > (celda.Ancho * celda.Largo)) || !ultimaFila )
                        return i;
                }
                else
                {

                    // Si Esta vacía, pero no entran celdas, continuo.
                    if ( (areaOptimizacion[i].VerticeDerechaArriba.Y - areaOptimizacion[i].VerticeDerechaAbajo.Y ) < celda.Largo || (areaOptimizacion[i].VerticeDerechaArriba.X - areaOptimizacion[i].VerticeIzquierdaArriba.X) < celda.Ancho )
                        continue;
                    
                    //si el area no tiene muebles => esta vacia, asi que la retorno.
                    return i;
                }
            }

            return -1;
        }

        public int GetSubAreaConEspacioAncho(List<AreaOptimizacion> areaOptimizacion, Celda celda)
        {
            MueblesOptmizacion muebleOptimizacion    = new MueblesOptmizacion();
            Model.ViewModels.Vector2 izquierdaArriba = new Model.ViewModels.Vector2();
            Model.ViewModels.Vector2 derechaArriba   = new Model.ViewModels.Vector2();
            Model.ViewModels.Vector2 izquierdaAbajo  = new Model.ViewModels.Vector2();
            Model.ViewModels.Vector2 derechaAbajo    = new Model.ViewModels.Vector2();
            double xMax;
            double yMin;

            for (int i = 0; i < areaOptimizacion.Count; i++)
            {
                if (areaOptimizacion[i].MueblesList.Any())
                {
                    yMin = areaOptimizacion[i].MueblesList.Select(x => x.VerticeDerechaAbajo.Y).Min();

                    List<Model.ViewModels.Vector2> listVertices = areaOptimizacion[i].MueblesList.Select(x => x.VerticeDerechaAbajo).Where(x => x.Y == yMin).ToList();
                    xMax = listVertices.Select(y => y.X).Max();
                    
                    // Vertice derechaAbajo del ultimo mueble agregado
                    derechaAbajo    = listVertices.Where(y => y.X == xMax).Select(y => y).First();
                    derechaArriba   = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeDerechaArriba).First();
                    izquierdaAbajo  = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeIzquierdaAbajo).First();
                    izquierdaArriba = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeIzquierdaArriba).First();

                    //Si coinciden los vertices => el area esta llena => paso a la siguiente area.
                    if (derechaAbajo == areaOptimizacion[i].VerticeDerechaAbajo)
                        continue;

                    //if ((areaOptimizacion[i].Area - areaOptimizacion[i].MueblesList.Sum(x => x.Area)) > (celda.Ancho * celda.Largo))
                    
                    double areaRestoFila = (Math.Abs(derechaArriba.Y - derechaAbajo.Y)) * (Math.Abs(derechaAbajo.X - areaOptimizacion[i].VerticeDerechaAbajo.X));
                    bool   ultimaFila    = (Math.Abs(areaOptimizacion[i].VerticeDerechaAbajo.Y - yMin)) < celda.Largo;
                  
                    if ((areaRestoFila > (celda.Ancho * celda.Largo)) || !ultimaFila)
                        return i;
                }
                else
                {

                    // Si Esta vacía, pero no entran celdas, continuo.
                    if ((areaOptimizacion[i].VerticeDerechaArriba.Y - areaOptimizacion[i].VerticeDerechaAbajo.Y) < celda.Largo || (areaOptimizacion[i].VerticeDerechaArriba.X - areaOptimizacion[i].VerticeIzquierdaArriba.X) < celda.Ancho)
                        continue;

                    //si el area no tiene muebles => esta vacia, asi que la retorno.
                    return i;
                }

            }

            return -1;
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
            double minimoX = vertices.Select(x => x.Position.X).Min();

            List<netDxf.Vector2> verticesSuperiores = vertices
                                                        .Select(coordenadas => coordenadas.Position)
                                                        .Where(coordenadas => coordenadas.Y == maximoY)
                                                        .ToList();

            netDxf.Vector2 derechaArriba = new netDxf.Vector2();
            netDxf.Vector2 IzquierdaArriba = new netDxf.Vector2();

            if (verticesSuperiores.Count > 0)
            {
                //if (verticesSuperiores.ElementAt(0).X > verticesSuperiores.ElementAt(1).X)
                //{
                //    derechaArriba = verticesSuperiores.ElementAt(0);
                //    IzquierdaArriba = verticesSuperiores.ElementAt(1);
                //}
                //else
                //{
                //    IzquierdaArriba = verticesSuperiores.ElementAt(0);
                //    derechaArriba = verticesSuperiores.ElementAt(1);
                //}
                derechaArriba.X = verticesSuperiores.Select(x => x.X).Max();
                derechaArriba.Y = maximoY;
                IzquierdaArriba.X = verticesSuperiores.Select(x => x.X).Min();
                IzquierdaArriba.Y = maximoY;
            }
            List<netDxf.Vector2> verticesInferiores = vertices
                                            .Select(coordenadas => coordenadas.Position)
                                            .Where(coordenadas => coordenadas.Y == minimoY)
                                            .ToList();

            netDxf.Vector2 derechaAbajo = new netDxf.Vector2();
            netDxf.Vector2 izquierdaAbajo = new netDxf.Vector2();

            if (verticesInferiores.Count > 0)
            {
                //if (verticesInferiores.ElementAt(0).X > verticesInferiores.ElementAt(1).X)
                //{
                //    derechaAbajo = verticesInferiores.ElementAt(0);
                //    izquierdaAbajo = verticesInferiores.ElementAt(1);
                //}
                //else
                //{
                //    izquierdaAbajo = verticesInferiores.ElementAt(0);
                //    derechaAbajo = verticesInferiores.ElementAt(1);
                //}
                derechaAbajo.X = verticesInferiores.Select(x => x.X).Max();
                derechaAbajo.Y = minimoY;
                izquierdaAbajo.X = verticesInferiores.Select(x => x.X).Min();
                izquierdaAbajo.Y = minimoY;
            }
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


            if ((centralTemporalUno.X == 0 || centralTemporalUno.Y == 0)
                && (centralTemporalDos.X == 0 || centralTemporalDos.Y == 0))
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

                areaOptimizacion.Ancho = Math.Abs(areaOptimizacion.VerticeDerechaAbajo.X - areaOptimizacion.VerticeIzquierdaAbajo.X);
                areaOptimizacion.Largo = Math.Abs(areaOptimizacion.VerticeDerechaArriba.Y - areaOptimizacion.VerticeDerechaAbajo.Y);

                areaOptimizacion.Area = areaOptimizacion.Ancho * areaOptimizacion.Largo;

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

                areaOptimizacion.VerticeIzquierdaAbajo.X = IzquierdaArriba.X;
                areaOptimizacion.VerticeIzquierdaAbajo.Y = centralMin.Y;

                areaOptimizacion.VerticeDerechaAbajo.X = derechaArriba.X;
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

                areaOptimizacion.VerticeIzquierdaArriba.X = izquierdaAbajo.X;
                areaOptimizacion.VerticeIzquierdaArriba.Y = centralMin.Y;

                areaOptimizacion.VerticeDerechaArriba.X = derechaAbajo.X;
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

        public double GetAnchoPasillo(LwPolyline lwPolyline, int sentido, int factorEscala)
        {
            int CantidadPersonas = cantidadPersonas == null ? 0 : (int)cantidadPersonas;

            if (CantidadPersonas <= 30)
            {
                return 1.10 * factorEscala;
            }
            else
            {
                if (CantidadPersonas > 30 && CantidadPersonas <= 50)
                {
                    return 1.20 * factorEscala;
                }
                else
                {
                    return (1.20 + ((CantidadPersonas - 50) / 15) * 0.15) * factorEscala;
                }
            }
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
