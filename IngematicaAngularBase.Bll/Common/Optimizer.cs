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
using netDxf.Tables;
using netDxf.Collections;
using AutoMapper;

namespace IngematicaAngularBase.Bll.Common
{
    public class Optimizer
    {
        private DxfDocument initialFlat;
        private List<Mueble> muebleList;
        private List<OptimizacionMueble> muebleCantidadList;
        private bool optimizarCosto;
        private double costoMaximo;
        private string escala;
        public int? cantidadPersonas;
        public bool registrarEnHistorial;
        public string Nombre;

        public Optimizer(DxfDocument dxfDocument, bool optimizarCostoParam, double costoMaximoParam, List<OptimizacionMueble> muebleCantidadListParam, List<Mueble> muebleListParam, string escalaParam, int? cantidadPersonasParam, bool pRegistrarEnHistorial, string pNombre)
        {
            this.initialFlat = dxfDocument;
            this.optimizarCosto = optimizarCostoParam;
            this.costoMaximo = costoMaximoParam;
            this.muebleCantidadList = muebleCantidadListParam;
            this.muebleList = muebleListParam;
            this.escala = escalaParam;
            this.cantidadPersonas = cantidadPersonasParam;
            this.registrarEnHistorial = pRegistrarEnHistorial;
            this.Nombre = pNombre;
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
            int countInicial = 0;
            bool indicaInvertir = false;
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

                    while (muebleListTemp.Count > 0)// && hayEspacio)
                    {
                        celda = GetTamañoMaximoCelda(muebleListTemp, (int)sentido, indicaInvertir);
                        index = UbicarMuebles(areaOptmizacion, muebleListTemp, (int)sentido, anchoPasillos, celda);

                        if (index == -1)
                        {
                            // Compacta la lista de zonasOcupadas y retorna si queda lugar libre en algun subarea
                            hayEspacio = Compactar(areaOptmizacion, (int)sentido, muebleListTemp, indicaInvertir);

                            if (!hayEspacio)
                            {
                                if (!indicaInvertir) // Se intenta ubicar los muebles mas pequeños antes de terminar
                                    indicaInvertir = true;
                                else
                                {
                                    indicaInvertir = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (muebleListTemp.Count() > 0) // Quedan muebles
                    {
                        //Insertar muebles en huecos
                        UbicarMueblesEnHuecos(areaOptmizacion, muebleListTemp, celda);
                    }

                    // Des-Alisar celdas y agregar los muebles de la lista de huecos a la de muebles
                    UnificarMueblesYHuecos(areaOptmizacion, (int)sentido);
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

        public void UnificarMueblesYHuecos(List<AreaOptimizacion> areaOptmizacion, int sentido)
        {
            MueblesOptmizacion muebleAux = new MueblesOptmizacion();
            MuebleBusiness mb = new MuebleBusiness();

            // Des-Alisar muebles
            foreach (AreaOptimizacion area in areaOptmizacion)
            {
                for (int i = 0; i < area.MueblesList.Count(); i++)
                {
                    muebleAux = area.MueblesList.ElementAt(i);
                    area.MueblesList[i] = mb.AjustarTamanio(ref muebleAux);
                }
            }

            foreach (AreaOptimizacion area in areaOptmizacion)
            {
                foreach (MueblesOptmizacion hueco in area.HuecosList)
                {
                    if (hueco.Mueble != null)
                    {
                        area.MueblesList.Add(hueco);
                    }
                }
            }
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

            foreach (Dimension dim in initialFlat.Dimensions)
            {
                Dimension dimClon = (Dimension)dim.Clone();
                dxfFinal.AddEntity(dimClon);
            }


            OptimizacionHistorialViewModel optimizacionHistorialViewModel = new OptimizacionHistorialViewModel();
            optimizacionHistorialViewModel.CantidadPersonas = this.cantidadPersonas;
            optimizacionHistorialViewModel.CostoMaximo = this.costoMaximo;
            optimizacionHistorialViewModel.Escala = this.escala;
            optimizacionHistorialViewModel.IdOptimizacionHistorial = -1;
            optimizacionHistorialViewModel.OptimizacionHistorialArea = new List<OptimizacionHistorialAreaViewModel>();

            List<OptimizacionMueblesViewModel> listMuebles = new List<OptimizacionMueblesViewModel>();

            foreach(OptimizacionMueble optMueble in muebleCantidadList)
            {
                OptimizacionMueblesViewModel optMueblView = new OptimizacionMueblesViewModel();
                optMueblView.IdOptimizacionHistorial = -1;
                optMueblView.IdOptimizacionMuebles = -1;
                optMueblView.Cantidad = optMueble.Cantidad;
                optMueblView.Mueble = (muebleList.Where(x => x.IdMueble == optMueble.IdMueble).First()).Nombre;
                listMuebles.Add(optMueblView);
            }

            optimizacionHistorialViewModel.OptimizacionMuebles = listMuebles;

            foreach (List<AreaOptimizacion> areaList in areaOptimizacion)
            {
                foreach (AreaOptimizacion area in areaList)
                {
                    OptimizacionHistorialAreaViewModel optimizacionHistorialAreaViewModel = new OptimizacionHistorialAreaViewModel();
                    optimizacionHistorialAreaViewModel.IdOptimizacionHistorial = -1;
                    optimizacionHistorialAreaViewModel.IdOptimizacionHistorialArea = -1;
                    optimizacionHistorialAreaViewModel.VerticeDerechaAbajoX = area.VerticeDerechaAbajo.X;
                    optimizacionHistorialAreaViewModel.VerticeDerechaAbajoY = area.VerticeDerechaAbajo.Y;
                    optimizacionHistorialAreaViewModel.VerticeDerechaArribaX = area.VerticeDerechaArriba.X;
                    optimizacionHistorialAreaViewModel.VerticeDerechaArribaY = area.VerticeDerechaArriba.Y;
                    optimizacionHistorialAreaViewModel.VerticeIzquierdaAbajoX = area.VerticeIzquierdaAbajo.X;
                    optimizacionHistorialAreaViewModel.VerticeIzquierdaAbajoY = area.VerticeIzquierdaAbajo.Y;
                    optimizacionHistorialAreaViewModel.VerticeIzquierdaArribaX = area.VerticeIzquierdaArriba.X;
                    optimizacionHistorialAreaViewModel.VerticeIzquierdaArribaY = area.VerticeIzquierdaArriba.Y;
                    optimizacionHistorialAreaViewModel.OptimizacionHistorialAreaMueble = new List<OptimizacionHistorialAreaMuebleViewModel>();

                    foreach (MueblesOptmizacion mueble in area.MueblesList)
                    {
                        OptimizacionHistorialAreaMuebleViewModel optimizacionHistorialAreaMuebleViewModel = new OptimizacionHistorialAreaMuebleViewModel();
                        optimizacionHistorialAreaMuebleViewModel.IdOptimizacionHistorailAreaMueble = -1;
                        optimizacionHistorialAreaMuebleViewModel.IdOptimizacionHistorialArea = -1;
                        optimizacionHistorialAreaMuebleViewModel.VerticeDerechaAbajoX = mueble.VerticeDerechaAbajo.X;
                        optimizacionHistorialAreaMuebleViewModel.VerticeDerechaAbajoY = mueble.VerticeDerechaAbajo.Y;
                        optimizacionHistorialAreaMuebleViewModel.VerticeDerechaArribaX = mueble.VerticeDerechaArriba.X;
                        optimizacionHistorialAreaMuebleViewModel.VerticeDerechaArribaY = mueble.VerticeDerechaArriba.Y;
                        optimizacionHistorialAreaMuebleViewModel.VerticeIzquierdaAbajoX = mueble.VerticeIzquierdaAbajo.X;
                        optimizacionHistorialAreaMuebleViewModel.VerticeIzquierdaAbajoY = mueble.VerticeIzquierdaAbajo.Y;
                        optimizacionHistorialAreaMuebleViewModel.VerticeIzquierdaArribaX = mueble.VerticeIzquierdaArriba.X;
                        optimizacionHistorialAreaMuebleViewModel.VerticeIzquierdaArribaY = mueble.VerticeIzquierdaArriba.Y;
                        //optimizacionHistorialAreaMuebleViewModel.IdMueble = mueble.Mueble.IdMueble;

                        if (!mueble.Mueble.PoseeRadio)
                        {
                            List<LwPolylineVertex> lpVertex = new List<LwPolylineVertex>();
                            lpVertex.Add(mb.ConvertVertex(mueble.VerticeIzquierdaAbajo));
                            lpVertex.Add(mb.ConvertVertex(mueble.VerticeIzquierdaArriba));
                            lpVertex.Add(mb.ConvertVertex(mueble.VerticeDerechaArriba));
                            lpVertex.Add(mb.ConvertVertex(mueble.VerticeDerechaAbajo));
                            dxfFinal.AddEntity(new LwPolyline(lpVertex, true));


                            //// Test Image
                            //netDxf.Vector2 verticeIzquierdaArriba = new netDxf.Vector2();
                            //verticeIzquierdaArriba.X = mueble.VerticeIzquierdaArriba.X;
                            //verticeIzquierdaArriba.Y = mueble.VerticeIzquierdaArriba.Y;
                            //netDxf.Objects.ImageDefinition imageDefinition = new netDxf.Objects.ImageDefinition("C:\\Temp\\Muebles\\test_imagen.PNG");
                            //Image imagen = new Image(imageDefinition, verticeIzquierdaArriba, mueble.VerticeDerechaArriba.X - mueble.VerticeIzquierdaArriba.X, mueble.VerticeIzquierdaArriba.Y - mueble.VerticeIzquierdaAbajo.Y);
                            //dxfFinal.AddEntity(imagen);


                            //// Test Image
                            //netDxf.Vector2 verticeIzquierdaArriba = new netDxf.Vector2();
                            //verticeIzquierdaArriba.X = mueble.VerticeIzquierdaArriba.X;
                            //verticeIzquierdaArriba.Y = mueble.VerticeIzquierdaArriba.Y;
                            //netDxf.Objects.ImageDefinition imageDefinition = new netDxf.Objects.ImageDefinition("C:\\Temp\\Muebles\\test_imagen.PNG");
                            //Image imagen = new Image(imageDefinition, verticeIzquierdaArriba, mueble.VerticeDerechaArriba.X - mueble.VerticeIzquierdaArriba.X, mueble.VerticeIzquierdaArriba.Y - mueble.VerticeIzquierdaAbajo.Y);
                            //dxfFinal.AddEntity(imagen);

                            /****************************
                            netDxf.Objects.ImageDefinition imageDefinition = new netDxf.Objects.ImageDefinition("C:\\Users\\Usuario\\Downloads\\21107733_336392926784075_6154084554823958528_a.jpg");
                            Image image = new Image(imageDefinition, Vector3.Zero, imageDefinition.Width, imageDefinition.Height);
                            dxfFinal.AddEntity(image);
                            *************************************/

                            /*****************************************


                            netDxf.Objects.ImageDefinition imageDef2 = new netDxf.Objects.ImageDefinition("C:\\Users\\Usuario\\Downloads\\21107733_336392926784075_6154084554823958528_a.jpg", "MyImage");
                            Image image2 = new Image(imageDef2, new Vector3(0, 500, 0), 100, 100);
                            Image image3 = new Image(imageDef2, new Vector3(500, 500, 0), 100, 100);

                            Block block = new Block("ImageBlock");
                            block.Entities.Add(image2);
                            block.Entities.Add(image3);
                            Insert insert = new Insert(block, new Vector3(0, 100, 0));

                            

                            dxfFinal.AddEntity(insert);


                            /*****************************************************************/

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

                        optimizacionHistorialAreaViewModel.OptimizacionHistorialAreaMueble.Add(optimizacionHistorialAreaMuebleViewModel);
                    }

                    optimizacionHistorialViewModel.OptimizacionHistorialArea.Add(optimizacionHistorialAreaViewModel);
                }
            }
            //dxfFinal.Save(filename); // LINEA PARA TESTING - BORRAR 

            if (this.registrarEnHistorial)
                GrabarHistorial(optimizacionHistorialViewModel);

            return dxfFinal;
        }

        public void GrabarHistorial(OptimizacionHistorialViewModel optimizacionHistorialViewModel)
        {
            optimizacionHistorialViewModel.Nombre = this.Nombre;
            Mapper.CreateMap<OptimizacionHistorialViewModel, OptimizacionHistorial>().IgnoreAllPropertiesWithAnInaccessibleSetter();
            Mapper.CreateMap<OptimizacionHistorialAreaViewModel, OptimizacionHistorialArea>().IgnoreAllPropertiesWithAnInaccessibleSetter();
            Mapper.CreateMap<OptimizacionHistorialAreaMuebleViewModel, OptimizacionHistorialAreaMueble>().IgnoreAllPropertiesWithAnInaccessibleSetter();
            Mapper.CreateMap<OptimizacionMueblesViewModel, OptimizacionMuebles>().IgnoreAllPropertiesWithAnInaccessibleSetter();


            OptimizacionHistorial optimizacionHistorial = Mapper.Map<OptimizacionHistorialViewModel, OptimizacionHistorial>(optimizacionHistorialViewModel);
            
            //agregar validaciones y auditoria
            using (var context = new Entities())
            {
                //var item = context.Set<GrupoConceptoFacturacion>().AsNoTracking().FirstOrDefault(x => x.Nombre == entity.Nombre);
                //if (item != null)
                //    throw new CustomApplicationException("Ya existe un grupo concepto de facturación con el mismo nombre.");


                //if (!entity.GrupoConceptoFacturacionClienteConcepto.Any())
                //    throw new CustomApplicationException("Una grupo concepto de facturación debe tener un concepto de facturación.");

                //entity.FechaAlta = DateTime.Now;
                context.OptimizacionHistorial.Add(optimizacionHistorial);
                context.SaveChanges();

            }
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
            //int countInicial = muebleList.Count();
            
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

        public void UbicarMueblesEnHuecos(List<AreaOptimizacion> areaOptimizacion, List<Mueble> muebleList, Celda celda)
        {
            MueblesOptmizacion muebleAux = new MueblesOptmizacion();
            MuebleBusiness mb = new MuebleBusiness();
            double derechaX = 0;
            int countInicial = 0;

            for (int j = 0; j < areaOptimizacion.Count(); j++)
            {
                countInicial = areaOptimizacion[j].HuecosList.Count();
                if (countInicial > 0)
                {
                    //Comparar tamaño celda con cada hueco
                    for (int i = 0; i < areaOptimizacion[j].HuecosList.Count(); i++)
                    {
                        if (muebleList.Count() == 0)
                            break;

                        if (areaOptimizacion[j].HuecosList.ElementAt(i).Ancho >= celda.Ancho && areaOptimizacion[j].HuecosList.ElementAt(i).Largo >= celda.Largo && areaOptimizacion[j].HuecosList.ElementAt(i).Mueble == null)
                        {
                            Mueble mueble = new Mueble();
                            decimal Largo = (decimal)celda.Largo;
                            int h = 0;
                            mueble = muebleList.First();

                            // Depende que celda estoy usando
                            if (mueble.Largo > Largo)
                            {
                                h = muebleList.FindIndex(x => x.Largo == Largo);
                                mueble = muebleList[h];
                            }
                            else
                                h = 0;

                            areaOptimizacion[j].HuecosList.ElementAt(i).Mueble = mueble;
                            areaOptimizacion[j].HuecosList.ElementAt(i).Mueble.Activo = true;

                            muebleList.RemoveAt(h);


                            // Comparar Ancho y Largo del Hueco con el del mueble
                            if (Math.Round(areaOptimizacion[j].HuecosList.ElementAt(i).Ancho, 2) > Math.Round(celda.Ancho, 2))
                            {
                                // Guardar Hueco en Largo
                                derechaX = areaOptimizacion[j].HuecosList.ElementAt(i).VerticeDerechaAbajo.X;
                                muebleAux = areaOptimizacion[j].HuecosList.ElementAt(i);
                                GuardarHuecoLargo(mb.AjustarTamanio(ref muebleAux), areaOptimizacion[j].HuecosList, derechaX);
                            }
                            else
                            {
                                if (areaOptimizacion[j].HuecosList.ElementAt(i).Largo > celda.Largo)
                                {
                                    //TODO: Guardar Hueco en Ancho
                                }
                            }
                        }
                    }
                }
            }
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
            decimal Largo = (decimal)celda.Largo;

            xMaxArea = areaOptimizacion.VerticeDerechaAbajo.X;

            if (areaOptimizacion.MueblesList.Count == 0)
            {
                #region
                // Area Vacía
                #endregion
                xMax            = areaOptimizacion.VerticeIzquierdaArriba.X;
                izquierdaAbajo  = areaOptimizacion.VerticeIzquierdaArriba;
                derechaAbajo.X  = areaOptimizacion.VerticeIzquierdaArriba.X + celda.Ancho;
                derechaAbajo.Y  = areaOptimizacion.VerticeIzquierdaArriba.Y;
            }
            else
            {
                double filaX = areaOptimizacion.MueblesList.First().VerticeIzquierdaArriba.X;
                double maximoX = areaOptimizacion.MueblesList.First().VerticeDerechaArriba.X;
                int ifila = 0;
                for (int i = 1; i <= areaOptimizacion.MueblesList.Count(); i++)
                {
                    if (i == areaOptimizacion.MueblesList.Count() || areaOptimizacion.MueblesList[i].VerticeIzquierdaArriba.X != filaX)
                    {
                        //Cambio la fila 
                        if(i < areaOptimizacion.MueblesList.Count())
                            filaX = areaOptimizacion.MueblesList[i].VerticeIzquierdaArriba.X;
                        if (maximoX < areaOptimizacion.MueblesList.ElementAt(i-1).VerticeDerechaAbajo.X)
                            maximoX = areaOptimizacion.MueblesList.ElementAt(i-1).VerticeDerechaAbajo.X;
                        //Reviso el ultimo elemento de la anterior, si hay espacio para agregr mas celdas
                        if ( areaOptimizacion.MueblesList[i - 1].VerticeDerechaAbajo.Y - areaOptimizacion.VerticeIzquierdaAbajo.Y >= celda.Largo
                            && areaOptimizacion.MueblesList[i - 1].Ancho >= celda.Ancho)
                        {
                            izquierdaAbajo.X = areaOptimizacion.MueblesList[i - 1].VerticeIzquierdaAbajo.X;
                            izquierdaAbajo.Y = areaOptimizacion.MueblesList[i - 1].VerticeIzquierdaAbajo.Y;
                            //izquierdaArriba.X = areaOptimizacion.MueblesList[i - 1].VerticeDerechaArriba.X;
                            //izquierdaArriba.Y = areaOptimizacion.MueblesList[i - 1].VerticeDerechaArriba.Y;

                            // Completar fila
                            if (izquierdaAbajo.X + celda.Ancho <= areaOptimizacion.VerticeDerechaAbajo.X &&
                                izquierdaAbajo.Y - celda.Largo >= areaOptimizacion.VerticeIzquierdaAbajo.Y)
                            {

                                celdaList = GetCeldaList(xMaxArea, celda, izquierdaAbajo, areaOptimizacion.VerticeDerechaAbajo.Y, muebleList.Select(x => x.Largo).Where(x => x <= Largo).Count());

                                #region
                                //verificar que muebleList y areaOptimizacion se esten pasando por referencia.
                                #endregion
                                foreach (MueblesOptmizacion celdaTemp in celdaList)
                                    IncertarMuebleCelda(celdaTemp, muebleList, areaOptimizacion);
                                areaOptimizacion.MueblesList = areaOptimizacion.MueblesList.OrderBy(mueble => mueble.VerticeIzquierdaArriba.X).ThenByDescending(mueble => mueble.VerticeIzquierdaArriba.Y).ToList();
                                i += celdaList.Count();
                                AlisarLargo(areaOptimizacion, ifila, i, maximoX, false);
                            }
                        }
                        ifila = i;
                    }
                    else
                    {
                        if (maximoX < areaOptimizacion.MueblesList.ElementAt(i).VerticeDerechaAbajo.X)
                            maximoX = areaOptimizacion.MueblesList.ElementAt(i).VerticeDerechaAbajo.X;
                    }
                }

                // Ordenar lista de mayor a menor Y y de menor a mayor X dentro de eso
                // para que funcione el corte de control por fila
                areaOptimizacion.MueblesList = areaOptimizacion.MueblesList.OrderBy(mueble => mueble.VerticeIzquierdaArriba.X).ThenByDescending(mueble => mueble.VerticeIzquierdaArriba.Y).ToList();

                xMax = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();

                // Checkear si la fila anterior es un pasillo, sino insertar uno
                if (Math.Round(areaOptimizacion.MueblesList.Select(mueble => mueble).Where(mueble => mueble.VerticeDerechaAbajo.X == xMax).First().Largo,2) != Math.Round(areaOptimizacion.Largo,2))
                {
                    // Checkeo para que no inserte celdas mas pequeñas q pasillos pegada a otra fila de celdas
                    if (!InsertarPasilloEnLargo(areaOptimizacion, anchoPasillos, xMax))
                        return;

                    xMax = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();
                }


                //xMax           = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();
                //derechaAbajo   = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo).Where(x => x.X == xMax).Min();
                //izquierdaAbajo = areaOptimizacion.MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeIzquierdaAbajo).First();
            }


            #region
            //si no valido esto pueden quedar 2 pasillos contiguos
            #endregion
            if (muebleList.Count > 0 )//&& areaOptimizacion.MueblesList.Count != 0)
            {
                do
                {

                    //xMax = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();

                    #region
                    //Genera una nueva fila de celdas
                    //usa xMax porque en caso de que quede formato cerrucho x compactacion, se desprecia una pequeña parte del plano
                    #endregion
                    celdaList = GetCeldaList(xMaxArea, celda, xMax, areaOptimizacion, muebleList.Select(x => x.Largo).Where(x => x <= Largo).Count());

                    #region
                    //tanto GetCeldaList como InsertarPasillo validan no pasarse del limite del subarea
                    #endregion
                    foreach (MueblesOptmizacion celdaTemp in celdaList)
                        IncertarMuebleCelda(celdaTemp, muebleList, areaOptimizacion);

                    xMax = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();

                    if (celdaList.Count != 0)
                    {
                        if (!InsertarPasilloEnLargo(areaOptimizacion, anchoPasillos, xMax))
                            return;
                        xMax = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();
                    }

                } while (muebleList.Count > 0 && celdaList.Count > 0);
                #region
                //con celdaList > 0 valido que se haya incertado algo en la iteracion anterior
                #endregion
            }
        }

        public bool InsertarPasilloEnLargo(AreaOptimizacion areaOptimizacion, double anchoPasillos, double xMax)
        {
            if (xMax + anchoPasillos <= areaOptimizacion.VerticeDerechaAbajo.X)
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
                return true;
            }
            else
                return false;
        }

        public void IncertarMuebleCelda(MueblesOptmizacion celda, List<Mueble> muebleList, AreaOptimizacion areaOptimizacion)
        {

            int i = 0;

            if (muebleList.Any())
            {
                Mueble mueble = muebleList.First();
                decimal Largo = (decimal)celda.Largo;
                if (mueble.Largo > Largo)
                {
                    i = muebleList.FindIndex(x => x.Largo == Largo);
                    celda.Mueble = muebleList[i];
                    //celda.Mueble = muebleList.Select(x => x).Where(x => x.Largo == Largo).First();
                }
                else
                {
                    celda.Mueble = muebleList.First();
                    i = 0;
                }
                areaOptimizacion.MueblesList.Add(celda);
                muebleList.RemoveAt(i);


            }
        }

        public List<MueblesOptmizacion> GetCeldaList(double xMaxArea, Celda celda, double xMax, AreaOptimizacion areaOptimizacion, int maximaCant)
        {
            List<MueblesOptmizacion> celdaList = new List<MueblesOptmizacion>();


            if (xMax + celda.Ancho <= xMaxArea && celdaList.Count() < maximaCant)
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
                //celdaMueble.Largo = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                //celdaMueble.Ancho = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
                celdaMueble.Ancho = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                celdaMueble.Largo = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
                celdaMueble.Area = celdaMueble.Largo * celdaMueble.Ancho;

                celdaList.Add(celdaMueble);


                while (celdaMueble.VerticeIzquierdaAbajo.Y - celda.Largo >= areaOptimizacion.VerticeIzquierdaAbajo.Y && celdaList.Count() < maximaCant)
                {
                    celdaMueble = new MueblesOptmizacion();

                    VerticeIzquierdaArriba = new Model.ViewModels.Vector2();
                    VerticeIzquierdaArriba.X = celdaList.Last().VerticeIzquierdaAbajo.X;
                    VerticeIzquierdaArriba.Y = celdaList.Last().VerticeIzquierdaAbajo.Y;
                    celdaMueble.VerticeIzquierdaArriba = VerticeIzquierdaArriba;

                    VerticeDerechaArriba = new Model.ViewModels.Vector2();
                    VerticeDerechaArriba.X = celdaList.Last().VerticeDerechaAbajo.X;
                    VerticeDerechaArriba.Y = celdaList.Last().VerticeDerechaAbajo.Y;
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
                    //celdaMueble.Largo = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                    //celdaMueble.Ancho = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
                    celdaMueble.Ancho = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                    celdaMueble.Largo = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
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
        public List<MueblesOptmizacion> GetCeldaList(double xMaxArea, Celda celda, Model.ViewModels.Vector2 izquierdaAbajo, double limiteInferior, int maximaCant)
        {
            List<MueblesOptmizacion> celdaList = new List<MueblesOptmizacion>();
            if (izquierdaAbajo.X + celda.Ancho <= xMaxArea)
            {
                while (izquierdaAbajo.Y - celda.Largo >= limiteInferior && celdaList.Count() < maximaCant)
                {
                    MueblesOptmizacion celdaMueble = new MueblesOptmizacion();

                    Model.ViewModels.Vector2 VerticeIzquierdaArriba = new Model.ViewModels.Vector2();
                    VerticeIzquierdaArriba.X = izquierdaAbajo.X;
                    VerticeIzquierdaArriba.Y = izquierdaAbajo.Y;
                    celdaMueble.VerticeIzquierdaArriba = VerticeIzquierdaArriba;

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
                    //celdaMueble.Largo = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                    //celdaMueble.Ancho = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
                    celdaMueble.Ancho = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                    celdaMueble.Largo = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
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
            decimal Largo = (decimal)celda.Largo;

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
                double filaY = areaOptimizacion.MueblesList.First().VerticeIzquierdaArriba.Y;
                double minimoY = areaOptimizacion.MueblesList.First().VerticeDerechaAbajo.Y;
                int iFila = 0;
                for (int i = 1; i <= areaOptimizacion.MueblesList.Count(); i++)
                {
                    if (i == areaOptimizacion.MueblesList.Count() || areaOptimizacion.MueblesList[i].VerticeIzquierdaArriba.Y != filaY)
                    {
                        //Cambio la fila 
                        if (i < areaOptimizacion.MueblesList.Count())
                            filaY = areaOptimizacion.MueblesList[i].VerticeIzquierdaArriba.Y;
                        if (minimoY > areaOptimizacion.MueblesList.ElementAt(i-1).VerticeDerechaAbajo.Y)
                            minimoY = areaOptimizacion.MueblesList.ElementAt(i-1).VerticeDerechaAbajo.Y;
                        //Reviso el ultimo elemento de la anterior, si hay espacio para agregr mas celdas
                        if (areaOptimizacion.VerticeDerechaArriba.X - areaOptimizacion.MueblesList[i - 1].VerticeDerechaArriba.X >= celda.Ancho
                            && areaOptimizacion.MueblesList[i - 1].Largo >= celda.Largo)
                        {
                            izquierdaAbajo.X = areaOptimizacion.MueblesList[i - 1].VerticeDerechaAbajo.X;
                            izquierdaAbajo.Y = areaOptimizacion.MueblesList[i - 1].VerticeDerechaAbajo.Y;
                            izquierdaArriba.X = areaOptimizacion.MueblesList[i - 1].VerticeDerechaArriba.X;
                            izquierdaArriba.Y = areaOptimizacion.MueblesList[i - 1].VerticeDerechaArriba.Y;

                            // Completar fila
                            if (izquierdaAbajo.X + celda.Ancho <= areaOptimizacion.VerticeDerechaAbajo.X &&
                                izquierdaAbajo.Y >= areaOptimizacion.VerticeIzquierdaAbajo.Y)
                            {
                                celdaList = GetCeldaListHorizontal(xMaxArea, celda, izquierdaArriba, muebleList.Select(x => x.Largo).Where(x => x <= Largo).Count());

                                #region
                                //verificar que muebleList y areaOptimizacion se esten pasando por referencia.
                                #endregion
                                foreach (MueblesOptmizacion celdaTemp in celdaList)
                                    IncertarMuebleCelda(celdaTemp, muebleList, areaOptimizacion);

                                areaOptimizacion.MueblesList = areaOptimizacion.MueblesList.OrderByDescending(mueble => mueble.VerticeIzquierdaArriba.Y).ThenBy(mueble => mueble.VerticeIzquierdaArriba.X).ToList();
                                i += celdaList.Count();
                                AlisarAncho(areaOptimizacion, iFila, i, minimoY,false);

                            }
                        }
                        iFila = i;
                    }
                    else
                    {
                        if (minimoY > areaOptimizacion.MueblesList.ElementAt(i).VerticeDerechaAbajo.Y)
                            minimoY = areaOptimizacion.MueblesList.ElementAt(i).VerticeDerechaAbajo.Y;
                    }
                }
                // Ordenar lista de mayor a menor Y y de menor a mayor X dentro de eso
                // para que funcione el corte de control por fila
                areaOptimizacion.MueblesList = areaOptimizacion.MueblesList.OrderByDescending(mueble => mueble.VerticeIzquierdaArriba.Y).ThenBy(mueble => mueble.VerticeIzquierdaArriba.X).ToList();

                yMin = areaOptimizacion.MueblesList.Select(y => y.VerticeIzquierdaAbajo.Y).Min();

                // Checkear si la fila anterior es un pasillo, sino insertar uno
                if (Math.Round(areaOptimizacion.MueblesList.Select(mueble => mueble).Where(mueble => mueble.VerticeIzquierdaAbajo.Y == yMin).First().Ancho,2) != Math.Round(areaOptimizacion.Ancho,2))
                {
                    if (!InsertarPasilloEnAncho(areaOptimizacion, anchoPasillos, yMin))
                        return;

                    yMin = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.Y).Min();

                }

            }

            #region
            //si no valido esto pueden quedar 2 pasillos contiguos
            #endregion
            if (muebleList.Count > 0 ) // && areaOptimizacion.MueblesList.Count != 0 )
            {
                do
                {
                    //yMin = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.Y).Min();

                    #region
                    //Genera una nueva fila de celdas
                    //usa xMax porque en caso de que quede formato cerrucho x compactacion, se desprecia una pequeña parte del plano
                    #endregion
                    celdaList = GetCeldaListHorizontal(celda, yMin, areaOptimizacion, muebleList.Select(x => x.Largo).Where(x => x <= Largo).Count());

                    #region
                    //tanto GetCeldaList como InsertarPasillo validan no pasarse del limite del subarea
                    #endregion
                    foreach (MueblesOptmizacion celdaTemp in celdaList)
                        IncertarMuebleCelda(celdaTemp, muebleList, areaOptimizacion);

                    yMin = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.Y).Min();

                    if (celdaList.Count > 0) //Sino inserta dos a lo ultimo
                        if (!InsertarPasilloEnAncho(areaOptimizacion, anchoPasillos, yMin))
                            return;

                    yMin = areaOptimizacion.MueblesList.Select(x => x.VerticeDerechaAbajo.Y).Min();

                } while (muebleList.Count > 0 && celdaList.Count > 0);
                #region
                //con celdaList > 0 valido que se haya incertado algo en la iteracion anterior
                #endregion
            }
        }

        public List<MueblesOptmizacion> GetCeldaListHorizontal(Celda celda, double yMin, AreaOptimizacion areaOptimizacion, int maximaCant)
        {
            List<MueblesOptmizacion> celdaList = new List<MueblesOptmizacion>();

            if (yMin - celda.Largo >= areaOptimizacion.VerticeDerechaAbajo.Y && celdaList.Count() < maximaCant)
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
                celdaMueble.Ancho = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                celdaMueble.Largo = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
                celdaMueble.Area = celdaMueble.Largo * celdaMueble.Ancho;

                celdaList.Add(celdaMueble);


                while (celdaMueble.VerticeDerechaAbajo.X + celda.Ancho <= areaOptimizacion.VerticeDerechaAbajo.X && celdaList.Count() < maximaCant)
                {
                    celdaMueble = new MueblesOptmizacion();

                    VerticeIzquierdaArriba = new Model.ViewModels.Vector2();
                    VerticeIzquierdaArriba.X = celdaList.Last().VerticeDerechaArriba.X;
                    VerticeIzquierdaArriba.Y = celdaList.Last().VerticeDerechaArriba.Y;
                    celdaMueble.VerticeIzquierdaArriba = VerticeIzquierdaArriba;

                    VerticeIzquierdaAbajo = new Model.ViewModels.Vector2();
                    VerticeIzquierdaAbajo.X = celdaList.Last().VerticeDerechaAbajo.X;
                    VerticeIzquierdaAbajo.Y = celdaList.Last().VerticeDerechaAbajo.Y;
                    celdaMueble.VerticeIzquierdaAbajo = VerticeIzquierdaAbajo;

                    VerticeDerechaArriba = new Model.ViewModels.Vector2();
                    VerticeDerechaArriba.Y = VerticeIzquierdaArriba.Y;
                    VerticeDerechaArriba.X = VerticeIzquierdaArriba.X + celda.Ancho;
                    celdaMueble.VerticeDerechaArriba = VerticeDerechaArriba;

                    VerticeDerechaAbajo = new Model.ViewModels.Vector2();
                    VerticeDerechaAbajo.Y = VerticeIzquierdaAbajo.Y;
                    VerticeDerechaAbajo.X = VerticeIzquierdaAbajo.X + celda.Ancho;
                    celdaMueble.VerticeDerechaAbajo = VerticeDerechaAbajo;

                    celdaMueble.Mueble = new Mueble();
                    celdaMueble.Ancho = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                    celdaMueble.Largo = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
                    celdaMueble.Area = celdaMueble.Largo * celdaMueble.Ancho;

                    celdaList.Add(celdaMueble);
                }

            }

            return celdaList;
        }

        public bool InsertarPasilloEnAncho(AreaOptimizacion areaOptimizacion, double anchoPasillos, double yMin)
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

                pasillo.Largo = anchoPasillos;
                pasillo.Ancho = Math.Abs(VerticeDerechaArriba.X - VerticeIzquierdaArriba.X);
                pasillo.Area = pasillo.Ancho * pasillo.Largo;

                areaOptimizacion.MueblesList.Add(pasillo);

                return true;
            }
            else
                return false;
        }

        public List<MueblesOptmizacion> GetCeldaListHorizontal(double xMaxArea, Celda celda, Model.ViewModels.Vector2 izquierdaArriba, int maximaCant)
        {
            List<MueblesOptmizacion> celdaList = new List<MueblesOptmizacion>();

            while (izquierdaArriba.X + celda.Ancho <= xMaxArea && celdaList.Count() < maximaCant)
            {

                MueblesOptmizacion celdaMueble = new MueblesOptmizacion();
                Model.ViewModels.Vector2 VerticeIzquierdaArriba = new Model.ViewModels.Vector2();
                VerticeIzquierdaArriba.X = izquierdaArriba.X;
                VerticeIzquierdaArriba.Y = izquierdaArriba.Y;
                celdaMueble.VerticeIzquierdaArriba = VerticeIzquierdaArriba;

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
                celdaMueble.Ancho = Math.Abs(VerticeDerechaAbajo.X - VerticeIzquierdaAbajo.X);
                celdaMueble.Largo = Math.Abs(VerticeDerechaArriba.Y - VerticeDerechaAbajo.Y);
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
                    // Si se comprimió, revisa fila por fila, hasta encontrar espacio al final de la fila o se terminan las filas
                    double filaX = areaOptimizacion[i].MueblesList.First().VerticeIzquierdaArriba.X;
                    for (int j = 1; j < areaOptimizacion[i].MueblesList.Count(); j++)
                    {
                        if (areaOptimizacion[i].MueblesList[j].VerticeIzquierdaArriba.X != filaX)
                        {
                            //Cambio la fila 
                            filaX = areaOptimizacion[i].MueblesList[j].VerticeIzquierdaArriba.X;
                            //Reviso el ultimo elemento de la anterior, si hay espacio para agregar mas celdas
                            if (areaOptimizacion[i].MueblesList[j - 1].VerticeIzquierdaAbajo.Y - areaOptimizacion[i].VerticeIzquierdaAbajo.Y >= celda.Largo
                                && areaOptimizacion[i].MueblesList[j - 1].VerticeDerechaArriba.X - areaOptimizacion[i].MueblesList[j - 1].VerticeIzquierdaArriba.X >= celda.Ancho)
                                return i;
                        }
                    }

                    xMax = areaOptimizacion[i].MueblesList.Select(x => x.VerticeDerechaAbajo.X).Max();

                    List<Model.ViewModels.Vector2> listVertices = areaOptimizacion[i].MueblesList.Select(x => x.VerticeDerechaAbajo).Where(x => x.X == xMax).ToList();
                    yMin = listVertices.Select(y => y.Y).Min();
                    
                    // Vertice derechaAbajo del ultimo mueble agregado
                    derechaAbajo    = listVertices.Where(y => y.Y == yMin).Select(y => y).First();
                    derechaArriba   = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeDerechaArriba).First();
                    izquierdaAbajo  = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeIzquierdaAbajo).First();
                    izquierdaArriba = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeIzquierdaArriba).First();
                    
                    //if ((areaOptimizacion[i].Area - areaOptimizacion[i].MueblesList.Sum(x => x.Area)) > (celda.Ancho * celda.Largo))
                    double areaRestoFila = (Math.Abs(derechaAbajo.X - izquierdaAbajo.X)) * (Math.Abs(derechaAbajo.Y - areaOptimizacion[i].VerticeDerechaAbajo.Y));
                    bool   ultimaFila    = (Math.Abs(areaOptimizacion[i].VerticeDerechaAbajo.X - xMax)) < celda.Ancho;
                    bool   esPasillo     = (Math.Round(areaOptimizacion[i].MueblesList.Last().Largo, 2) == Math.Round(areaOptimizacion[i].Largo, 2));

                    if (((areaRestoFila > (celda.Ancho * celda.Largo))
                            && Math.Abs(izquierdaAbajo.X - derechaAbajo.X) >= celda.Ancho
                            && (Math.Abs(derechaAbajo.Y - areaOptimizacion[i].VerticeDerechaAbajo.Y)) >= celda.Largo)
                        || (!(ultimaFila) && esPasillo))
                        return i;

                    //// Verificar lista de Huecos
                    //if (HayEspacioEnHuecos(areaOptimizacion[i].HuecosList, celda))
                    //    return i;

                    //Si coinciden los vertices => el area esta llena => paso a la siguiente area.
                    if (derechaAbajo == areaOptimizacion[i].VerticeDerechaAbajo)
                        continue;

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
                    // Si se comprimió, revisa fila por fila, hasta encontrar espacio al final de la fila o se terminan las filas
                    double filaY = areaOptimizacion[i].MueblesList.First().VerticeIzquierdaArriba.Y;
                    for (int j = 1; j < areaOptimizacion[i].MueblesList.Count(); j++)
                    {
                        if (areaOptimizacion[i].MueblesList[j].VerticeIzquierdaArriba.Y != filaY)
                        {
                            //Cambio la fila 
                            filaY = areaOptimizacion[i].MueblesList[j].VerticeIzquierdaArriba.Y;
                            //Reviso el ultimo elemento de la anterior, si hay espacio para agregr mas celdas
                            if (areaOptimizacion[i].VerticeDerechaArriba.X - areaOptimizacion[i].MueblesList[j - 1].VerticeDerechaArriba.X >= celda.Ancho
                                && areaOptimizacion[i].MueblesList[j - 1].VerticeIzquierdaArriba.Y - areaOptimizacion[i].MueblesList[j - 1].VerticeIzquierdaAbajo.Y >= celda.Largo)
                                return i;
                        }
                    }
                    
                    // Verifico si hay espacio debajo de la última fila
                    yMin = areaOptimizacion[i].MueblesList.Select(x => x.VerticeDerechaAbajo.Y).Min();

                    List<Model.ViewModels.Vector2> listVertices = areaOptimizacion[i].MueblesList.Select(x => x.VerticeDerechaAbajo).Where(x => x.Y == yMin).ToList();
                    xMax = listVertices.Select(y => y.X).Max();
                    
                    // Vertice derechaAbajo del ultimo mueble agregado
                    derechaAbajo    = listVertices.Where(y => y.X == xMax).Select(y => y).First();
                    derechaArriba   = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeDerechaArriba).First();
                    izquierdaAbajo  = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeIzquierdaAbajo).First();
                    izquierdaArriba = areaOptimizacion[i].MueblesList.Where(x => x.VerticeDerechaAbajo == derechaAbajo).Select(x => x.VerticeIzquierdaArriba).First();


                    //if ((areaOptimizacion[i].Area - areaOptimizacion[i].MueblesList.Sum(x => x.Area)) > (celda.Ancho * celda.Largo))
                    
                    double areaRestoFila = (Math.Abs(derechaArriba.Y - derechaAbajo.Y)) * (Math.Abs(derechaAbajo.X - areaOptimizacion[i].VerticeDerechaAbajo.X));

                    bool   ultimaFila    = (Math.Abs(areaOptimizacion[i].VerticeDerechaAbajo.Y - yMin)) < celda.Largo;
                  
                    if (((areaRestoFila > (celda.Ancho * celda.Largo)) 
                            && Math.Abs(derechaArriba.Y - derechaAbajo.Y) >= celda.Largo
                            && (Math.Abs(derechaAbajo.X - areaOptimizacion[i].VerticeDerechaAbajo.X)) >= celda.Ancho) 
                        || !ultimaFila)
                        return i;

                    //// Verificar lista de Huecos
                    //if (HayEspacioEnHuecos(areaOptimizacion[i].HuecosList, celda))
                    //    return i;

                    //Si coinciden los vertices => el area esta llena => paso a la siguiente area.
                    if (derechaAbajo == areaOptimizacion[i].VerticeDerechaAbajo)
                        continue;
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


        public bool HayEspacioEnHuecos(List<MueblesOptmizacion> huecosList, Celda celda)
        {
            if (huecosList.Count() == 0)
                return false;
            foreach (MueblesOptmizacion hueco in huecosList)
            {
                //Comparar tamaño celda con cada hueco
                if (hueco.Ancho >= celda.Ancho && hueco.Largo >= celda.Largo && hueco.Mueble == null)
                    return true;
            }
            return false;
        }

        #region
        //Metodo que retorna el tamaño de la celda.
        //Recibe el sentido en que se ponen los muebles, porque si algun mueble posee radio mayor, 
        //se considera en el sentido de recorrido del plano.
        #endregion
        public Celda GetTamañoMaximoCelda(List<Mueble> muebles, int sentido, bool invertido)
        {
            Celda celda = new Celda();
            decimal? Largo = 0;
            decimal? Ancho = 0;
            decimal? RadioMayor = 0;
            decimal? RadioMenor = 0;

            if (!invertido)
            {
                Largo = muebles.Select(x => x.Largo + x.DistanciaParedes + x.DistanciaProximoMueble).Max();
                Ancho = muebles.Select(x => x.Ancho + x.DistanciaParedes + x.DistanciaProximoMueble).Max();
                RadioMayor = muebles.Select(x => x.RadioMayor + x.DistanciaParedes + x.DistanciaProximoMueble).Max();
                RadioMenor = muebles.Select(x => x.RadioMayor + x.DistanciaParedes + x.DistanciaProximoMueble).Max();
            }
            else
            {
                Largo = muebles.Select(x => x.Largo + x.DistanciaParedes + x.DistanciaProximoMueble).Min();
                Ancho = muebles.Select(x => x).Where(x => x.Largo == Largo).First().Ancho;
//                Ancho = muebles.Select(x => x.Ancho + x.DistanciaParedes + x.DistanciaProximoMueble).Min();
//                RadioMayor = muebles.Select(x => x.RadioMayor + x.DistanciaParedes + x.DistanciaProximoMueble).Max();
//                RadioMenor = muebles.Select(x => x.RadioMayor + x.DistanciaParedes + x.DistanciaProximoMueble).Max();
            }
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
                areaOptimizacion.HuecosList = new List<MueblesOptmizacion>();

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
                areaOptimizacion.HuecosList = new List<MueblesOptmizacion>();

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
                areaOptimizacion.HuecosList  = new List<MueblesOptmizacion>();

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

        public bool Compactar(List<AreaOptimizacion> areaOptimizacion, int sentido, List<Mueble> muebleListTemp, bool invertirCelda)
        {
            bool hayEspacio = false;

            foreach (AreaOptimizacion area in areaOptimizacion)
            {
                if ((int)sentido == 2)
                    hayEspacio = CompactarAncho(area, muebleListTemp, invertirCelda);
                else
                { 
                    hayEspacio = CompactarLargo(area, muebleListTemp, invertirCelda);
                    //hayEspacio = false;
                }
                if (hayEspacio)
                    return true;
            }

            return hayEspacio;
        }

        public bool CompactarLargo(AreaOptimizacion areaOptimizacion, List<Mueble> muebles, bool invertirCelda)
        {
            List<MueblesOptmizacion> huecos = new List<MueblesOptmizacion>();
            int index = -1;
            int ifila = 0;
            Celda celda = new Celda();
            MuebleBusiness mb = new MuebleBusiness();
            double minimoX = 0;
            double maximoX = 0;
            double FilaXOriginal = 0;
            MueblesOptmizacion muebleAux;

            // Checkear si el area esta vacía y entra una celda
            celda = GetTamañoMaximoCelda(muebles, 1, invertirCelda);

            if (areaOptimizacion.MueblesList.Count() == 0)
            {
                if (areaOptimizacion.Ancho >= celda.Ancho && areaOptimizacion.Largo >= celda.Largo)
                    return true;
                else
                    return false;
            }

            // Borrar lista de Huecos
            if (areaOptimizacion.HuecosList.Count() > 0)
                areaOptimizacion.HuecosList.Clear();
            
            // Primer mueble
            muebleAux = areaOptimizacion.MueblesList.First();
            areaOptimizacion.MueblesList[0] = mb.AjustarTamanio(ref muebleAux);
            minimoX = areaOptimizacion.MueblesList.First().VerticeIzquierdaArriba.X;
            maximoX = areaOptimizacion.MueblesList.First().VerticeDerechaArriba.X;
            FilaXOriginal = minimoX;

            for (int i = 1; i < areaOptimizacion.MueblesList.Count; i++)
            {

                if (areaOptimizacion.MueblesList.ElementAt(i).VerticeIzquierdaArriba.X == FilaXOriginal)
                {
                    // Compacto hacia arriba
                    // Asignar coordenadas de abajo de mueble anterior al siguiente
                    muebleAux = areaOptimizacion.MueblesList.ElementAt(i);
                    areaOptimizacion.MueblesList[i] = mb.DesplazarArriba(ref muebleAux, areaOptimizacion.MueblesList.ElementAt(i - 1).VerticeDerechaAbajo.Y);

                    if (minimoX < areaOptimizacion.MueblesList.ElementAt(i - 1).VerticeIzquierdaArriba.X)
                    {
                        muebleAux = areaOptimizacion.MueblesList.ElementAt(i - 1);
                        areaOptimizacion.MueblesList[(i - 1)] = mb.DesplazarIzquierda(ref muebleAux, minimoX); // Desplazo Izquierda el mueble anterior
                    }
                    muebleAux = areaOptimizacion.MueblesList.ElementAt(i);
                    areaOptimizacion.MueblesList[i] = mb.AjustarTamanio(ref muebleAux);
                    areaOptimizacion.MueblesList[i] = mb.DesplazarIzquierda(ref muebleAux, minimoX);

                    if (maximoX < areaOptimizacion.MueblesList.ElementAt(i).VerticeDerechaAbajo.X)
                        maximoX = areaOptimizacion.MueblesList.ElementAt(i).VerticeDerechaAbajo.X;
                }
                else
                {
                    // Termino la fila
                    AlisarLargo(areaOptimizacion, ifila, i, maximoX, true);
                    
                    // Comparar maximoX de la fila anterior con el primer elemento de esta
                    //if (maximoX < areaOptimizacion.MueblesList.ElementAt(i).VerticeDerechaArriba.X)
                    // Guardar FilaXOriginal
                    FilaXOriginal = areaOptimizacion.MueblesList.ElementAt(i).VerticeIzquierdaArriba.X;
                    // Mover el primer elemento
                    muebleAux = areaOptimizacion.MueblesList.ElementAt(i);
                    areaOptimizacion.MueblesList[i] = mb.DesplazarIzquierda(ref muebleAux, maximoX);

                    // Maximo para proxima fila
                    minimoX = areaOptimizacion.MueblesList.ElementAt(i - 1).VerticeDerechaAbajo.X;

                    muebleAux = areaOptimizacion.MueblesList.ElementAt(i);
                    areaOptimizacion.MueblesList[i] = mb.AjustarTamanio(ref muebleAux);
                    maximoX = areaOptimizacion.MueblesList.ElementAt(i).VerticeDerechaAbajo.X;
                    ifila = i;
                }
            }
            
            List<AreaOptimizacion> areaOptimizacionList = new List<AreaOptimizacion>();
            areaOptimizacionList.Add(areaOptimizacion);

            index = GetSubAreaConEspacioLargo(areaOptimizacionList, celda);
            if (index > -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void AlisarLargo(AreaOptimizacion areaOptimizacion, int initFila, int finFila, double maximoX, bool actualizarHuecos)
        {
            // Asignar maximoX a toda la fila
            for (int h = initFila; h < finFila; h++)
            {
                if (Math.Round(areaOptimizacion.MueblesList.ElementAt(h).VerticeDerechaAbajo.X, 2) < Math.Round(maximoX, 2))
                {
                    if(actualizarHuecos)
                        // Guardar hueco
                        GuardarHuecoLargo(areaOptimizacion.MueblesList.ElementAt(h), areaOptimizacion.HuecosList, maximoX);
                    
                    // Alisar fila
                    areaOptimizacion.MueblesList.ElementAt(h).VerticeDerechaAbajo.X = maximoX;
                    areaOptimizacion.MueblesList.ElementAt(h).VerticeDerechaArriba.X = maximoX;
                    areaOptimizacion.MueblesList.ElementAt(h).Ancho = maximoX - areaOptimizacion.MueblesList.ElementAt(h).VerticeIzquierdaArriba.X;
                }
            }
        }


        public void GuardarHuecoLargo(MueblesOptmizacion muebleOpt, List<MueblesOptmizacion> huecosList, double derechaX)
        {
            MueblesOptmizacion hueco = new MueblesOptmizacion();
            Model.ViewModels.Vector2 VerticeIzquierdaAbajo = new Model.ViewModels.Vector2();
            Model.ViewModels.Vector2 VerticeIzquierdaArriba = new Model.ViewModels.Vector2();
            Model.ViewModels.Vector2 VerticeDerechaArriba = new Model.ViewModels.Vector2();
            Model.ViewModels.Vector2 VerticeDerechaAbajo = new Model.ViewModels.Vector2();

            VerticeIzquierdaArriba.X = muebleOpt.VerticeDerechaArriba.X;
            VerticeIzquierdaArriba.Y = muebleOpt.VerticeDerechaArriba.Y;
            hueco.VerticeIzquierdaArriba = VerticeIzquierdaArriba;

            VerticeIzquierdaAbajo.X = muebleOpt.VerticeDerechaAbajo.X;
            VerticeIzquierdaAbajo.Y = muebleOpt.VerticeDerechaAbajo.Y;
            hueco.VerticeIzquierdaAbajo = VerticeIzquierdaAbajo;

            VerticeDerechaArriba.X = derechaX;
            VerticeDerechaArriba.Y = muebleOpt.VerticeDerechaArriba.Y;
            hueco.VerticeDerechaArriba = VerticeDerechaArriba;

            VerticeDerechaAbajo.X = derechaX;
            VerticeDerechaAbajo.Y = muebleOpt.VerticeDerechaAbajo.Y;
            hueco.VerticeDerechaAbajo = VerticeDerechaAbajo;

            hueco.Ancho = hueco.VerticeDerechaAbajo.X - hueco.VerticeIzquierdaAbajo.X;
            hueco.Largo = hueco.VerticeIzquierdaArriba.Y - hueco.VerticeIzquierdaAbajo.Y;
            huecosList.Add(hueco);
        }

        public bool CompactarAncho(AreaOptimizacion areaOptimizacion, List<Mueble> muebles, bool invertirCelda)
        {
            //List<MueblesOptmizacion> huecos   = new List<MueblesOptmizacion>();
            int index = -1;
            int ifila = 0;
            Celda celda = new Celda();
            MuebleBusiness mb = new MuebleBusiness();
            double minimoY = 0;
            double maximoY = 0;
            double FilaYOriginal = 0;
            MueblesOptmizacion muebleAux;

            // Checkear si el area esta vacía y entra una celda
            celda = GetTamañoMaximoCelda(muebles, 1, invertirCelda);

            if (areaOptimizacion.MueblesList.Count() == 0)
            {
                if (areaOptimizacion.Ancho >= celda.Ancho && areaOptimizacion.Largo >= celda.Largo)
                    return true;
                else
                    return false;
            }

            // Borrar lista de Huecos
            if (areaOptimizacion.HuecosList.Count() > 0)
                areaOptimizacion.HuecosList.Clear();

            // Primer mueble
            muebleAux = areaOptimizacion.MueblesList.First();
            areaOptimizacion.MueblesList[0] = mb.AjustarTamanio(ref muebleAux);
            minimoY = areaOptimizacion.MueblesList.First().VerticeDerechaAbajo.Y;
            maximoY = areaOptimizacion.MueblesList.First().VerticeDerechaArriba.Y;
            FilaYOriginal = maximoY;

            for (int i = 1; i < areaOptimizacion.MueblesList.Count; i++)
            {
                
                if (areaOptimizacion.MueblesList.ElementAt(i).VerticeDerechaArriba.Y == FilaYOriginal)
                {
                    // Compacto hacia la izquierda
                    // Asignar coordenadas de derecha de mueble anterior al siguiente
                    muebleAux = areaOptimizacion.MueblesList.ElementAt(i);
                    areaOptimizacion.MueblesList[i] = mb.DesplazarIzquierda(ref muebleAux, areaOptimizacion.MueblesList.ElementAt(i-1).VerticeDerechaAbajo.X);

                    if (maximoY > areaOptimizacion.MueblesList.ElementAt(i - 1).VerticeDerechaArriba.Y)
                    {
                        muebleAux = areaOptimizacion.MueblesList.ElementAt(i - 1);
                        areaOptimizacion.MueblesList[(i - 1)] = mb.DesplazarArriba(ref muebleAux, maximoY); // Desplazo Arriba el mueble anterior
                    }
                    muebleAux = areaOptimizacion.MueblesList.ElementAt(i);
                    areaOptimizacion.MueblesList[i] = mb.AjustarTamanio(ref muebleAux);
                    areaOptimizacion.MueblesList[i] = mb.DesplazarArriba(ref muebleAux, maximoY);

                    if (minimoY > areaOptimizacion.MueblesList.ElementAt(i).VerticeDerechaAbajo.Y)
                        minimoY = areaOptimizacion.MueblesList.ElementAt(i).VerticeDerechaAbajo.Y;
                }
                else
                {
                    // Termino la fila
                    AlisarAncho(areaOptimizacion, ifila, i, minimoY, true);

                    // Guardar FilaYOriginal
                    FilaYOriginal = areaOptimizacion.MueblesList.ElementAt(i).VerticeDerechaArriba.Y;
                    // Mover el primer elemento
                    muebleAux = areaOptimizacion.MueblesList.ElementAt(i);
                    areaOptimizacion.MueblesList[i] = mb.DesplazarArriba(ref muebleAux, minimoY);
                    
                    // Maximo para proxima fila
                    maximoY = areaOptimizacion.MueblesList.ElementAt(i-1).VerticeDerechaAbajo.Y;

                    muebleAux = areaOptimizacion.MueblesList.ElementAt(i);
                    areaOptimizacion.MueblesList[i] = mb.AjustarTamanio(ref muebleAux);
                    minimoY = areaOptimizacion.MueblesList.ElementAt(i).VerticeDerechaAbajo.Y;
                    ifila = i;
                }
            }
            
            List<AreaOptimizacion> areaOptimizacionList = new List<AreaOptimizacion>();
            areaOptimizacionList.Add(areaOptimizacion);

            index = GetSubAreaConEspacioAncho(areaOptimizacionList, celda);
            if (index > -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void AlisarAncho(AreaOptimizacion areaOptimizacion, int initFila, int finFila, double minimoY, bool actualizarHuecos)
        {
            // Asignar minimoY a toda la fila
            for (int h = initFila; h < finFila; h++)
            {
                if (Math.Round(areaOptimizacion.MueblesList.ElementAt(h).VerticeDerechaAbajo.Y, 2) > Math.Round(minimoY, 2))
                {

                    if (actualizarHuecos)
                    {
                        // Guardar hueco
                        MueblesOptmizacion hueco = new MueblesOptmizacion();
                        Model.ViewModels.Vector2 VerticeIzquierdaAbajo = new Model.ViewModels.Vector2();
                        Model.ViewModels.Vector2 VerticeIzquierdaArriba = new Model.ViewModels.Vector2();
                        Model.ViewModels.Vector2 VerticeDerechaArriba = new Model.ViewModels.Vector2();
                        Model.ViewModels.Vector2 VerticeDerechaAbajo = new Model.ViewModels.Vector2();


                        VerticeIzquierdaArriba.X = areaOptimizacion.MueblesList.ElementAt(h).VerticeIzquierdaAbajo.X;
                        VerticeIzquierdaArriba.Y = areaOptimizacion.MueblesList.ElementAt(h).VerticeIzquierdaAbajo.Y;
                        hueco.VerticeIzquierdaArriba = VerticeIzquierdaArriba;

                        VerticeDerechaArriba.X = areaOptimizacion.MueblesList.ElementAt(h).VerticeDerechaAbajo.X;
                        VerticeDerechaArriba.Y = areaOptimizacion.MueblesList.ElementAt(h).VerticeDerechaAbajo.Y;
                        hueco.VerticeDerechaArriba = VerticeDerechaArriba;

                        VerticeIzquierdaAbajo.X = areaOptimizacion.MueblesList.ElementAt(h).VerticeIzquierdaAbajo.X;
                        VerticeIzquierdaAbajo.Y = minimoY;
                        hueco.VerticeIzquierdaAbajo = VerticeIzquierdaAbajo;

                        VerticeDerechaAbajo.X = areaOptimizacion.MueblesList.ElementAt(h).VerticeDerechaAbajo.X;
                        VerticeDerechaAbajo.Y = minimoY;
                        hueco.VerticeDerechaAbajo = VerticeDerechaAbajo;

                        hueco.Ancho = hueco.VerticeDerechaAbajo.X - hueco.VerticeIzquierdaAbajo.X;
                        hueco.Largo = hueco.VerticeIzquierdaArriba.Y - hueco.VerticeIzquierdaAbajo.Y;
                        areaOptimizacion.HuecosList.Add(hueco);
                    }
                    // Alisar fila
                    areaOptimizacion.MueblesList.ElementAt(h).VerticeDerechaAbajo.Y = minimoY;
                    areaOptimizacion.MueblesList.ElementAt(h).VerticeIzquierdaAbajo.Y = minimoY;
                    areaOptimizacion.MueblesList.ElementAt(h).Largo = areaOptimizacion.MueblesList.ElementAt(h).VerticeIzquierdaArriba.Y - minimoY;
                }
            }
        }

        /*  (2)
         
         //Particionar el espacio libre en el tamaño de la celda (incluye tamaño de la entidad y del espacio libre a su alrededor) y cargar una lista de "huecos libres"
         

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
