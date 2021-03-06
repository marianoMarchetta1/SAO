﻿using IngematicaAngularBase.Dal;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.Entities;
using IngematicaAngularBase.Model.ViewModels;
using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IngematicaAngularBase.Bll
{
    public class OptimizacionHistorialBusiness
    {
        public QueryResult<OptimizacionHistorialListViewModel> GetList(OptimizacionHistorialQuery query)
        {
            using (var context = new Entities())
            {
                OptimizacionHistorialDataAccess optimizacionHistorialDataAccess = new OptimizacionHistorialDataAccess(context);
                return optimizacionHistorialDataAccess.GetList(query);
            }
        }

        public void Delete(int id)
        {
            using (var context = new Entities())
            {
                var optimizacionHistorial = from optimizacionHistorialElement in context.Set<OptimizacionHistorial>()
                                            where optimizacionHistorialElement.IdOptimizacionHistorial == id
                                            select optimizacionHistorialElement;

                OptimizacionHistorial opHis = optimizacionHistorial.First();

                optimizacionHistorial = from optimizacionHistorialElement in context.Set<OptimizacionHistorial>()
                                        where optimizacionHistorialElement.Nombre == opHis.Nombre
                                        select optimizacionHistorialElement;

                List<int> idsOptimizacionHistorial = optimizacionHistorial.Select(x => x.IdOptimizacionHistorial).ToList();

                var optimizacionHistorialAreaList = from optimizacionHistorialAreaElement in context.Set<OptimizacionHistorialArea>()
                                                    where idsOptimizacionHistorial.Contains(optimizacionHistorialAreaElement.IdOptimizacionHistorial)
                                                    select optimizacionHistorialAreaElement;

                List<int> idsOptimizacionHistorialArea = optimizacionHistorialAreaList.Select(x => x.IdOptimizacionHistorialArea).ToList();

                var optimizacionHistorialAreaMuebleList = from optimizacionHistorialAreaMuebleElement in context.Set<OptimizacionHistorialAreaMueble>()
                                                          where idsOptimizacionHistorialArea.Contains(optimizacionHistorialAreaMuebleElement.IdOptimizacionHistorialArea)
                                                          select optimizacionHistorialAreaMuebleElement;

                var mueblesResult = from optimizacionMuebles in context.Set<OptimizacionMuebles>()
                                    where idsOptimizacionHistorial.Contains(optimizacionMuebles.IdOptimizacionHistorial)
                                    select optimizacionMuebles;

                mueblesResult.ToList().ForEach(x => context.Entry(x).State = EntityState.Deleted);
                optimizacionHistorialAreaMuebleList.ToList().ForEach(x => context.Entry(x).State = EntityState.Deleted);
                optimizacionHistorialAreaList.ToList().ForEach(x => context.Entry(x).State = EntityState.Deleted);
                optimizacionHistorial.ToList().ForEach(x => context.Entry(x).State = EntityState.Deleted);
                              
                context.SaveChanges();
            }
        }

        public OptimizacionHistorialViewModel GetById(int id)
        {
            MuebleBusiness mb = new MuebleBusiness();
            string path = System.Configuration.ConfigurationManager.AppSettings["TmpFiles"];

            using (var context = new Entities())
            {
                //Recorrer la lista y generar paths a retornar en pantalla
                OptimizacionHistorialDataAccess optimizacionHistorialDataAccess = new OptimizacionHistorialDataAccess(context);
                OptimizacionHistorialViewModel optimizacionHistorialViewModel = optimizacionHistorialDataAccess.GetById(id);
                List<OptimizacionHistorialViewModel> optimizacionHistorialViewModelDos = optimizacionHistorialDataAccess.GetByIdSinAgrupar(id);
                optimizacionHistorialViewModel.PathsImages = new List<string>();
                DateTime dateTimeNow = DateTime.Now;

                foreach (OptimizacionHistorialViewModel historico in optimizacionHistorialViewModelDos)
                {
                    
                    string pathTemp = path + "\\temp " + dateTimeNow.ToString("yyyyMMddHHmmss") + ".dxf";
                    string pathImage = System.Configuration.ConfigurationManager.AppSettings["TmpImageFiles"];
                    optimizacionHistorialViewModel.Paths.Add(pathTemp);
                    DxfDocument dxfFinal = new DxfDocument();

                    foreach (OptimizacionHistorialAreaViewModel optimizacionHistorialAreaViewModel in historico.OptimizacionHistorialArea)
                    {
                        List<LwPolylineVertex> verticesArea = new List<LwPolylineVertex>();

                        Model.ViewModels.Vector2 verticeIzqueirdaArriba = new Model.ViewModels.Vector2();
                        verticeIzqueirdaArriba.X = (double)optimizacionHistorialAreaViewModel.VerticeIzquierdaArribaX;
                        verticeIzqueirdaArriba.Y = (double)optimizacionHistorialAreaViewModel.VerticeIzquierdaArribaY;
                        verticesArea.Add(mb.ConvertVertex(verticeIzqueirdaArriba));

                        Model.ViewModels.Vector2 verticeDerechaArriba = new Model.ViewModels.Vector2();
                        verticeDerechaArriba.X = (double)optimizacionHistorialAreaViewModel.VerticeDerechaArribaX;
                        verticeDerechaArriba.Y = (double)optimizacionHistorialAreaViewModel.VerticeDerechaArribaY;
                        verticesArea.Add(mb.ConvertVertex(verticeDerechaArriba));

                        Model.ViewModels.Vector2 verticeDerechaAbajo = new Model.ViewModels.Vector2();
                        verticeDerechaAbajo.X = (double)optimizacionHistorialAreaViewModel.VerticeDerechaAbajoX;
                        verticeDerechaAbajo.Y = (double)optimizacionHistorialAreaViewModel.VerticeDerechaAbajoY;
                        verticesArea.Add(mb.ConvertVertex(verticeDerechaAbajo));

                        Model.ViewModels.Vector2 verticeIzqueirdaAbajo = new Model.ViewModels.Vector2();
                        verticeIzqueirdaAbajo.X = (double)optimizacionHistorialAreaViewModel.VerticeIzquierdaAbajoX;
                        verticeIzqueirdaAbajo.Y = (double)optimizacionHistorialAreaViewModel.VerticeIzquierdaAbajoY;
                        verticesArea.Add(mb.ConvertVertex(verticeIzqueirdaAbajo));

                        dxfFinal.AddEntity(new LwPolyline(verticesArea, true));

                        foreach(OptimizacionHistorialAreaMuebleViewModel mueble in optimizacionHistorialAreaViewModel.OptimizacionHistorialAreaMueble)
                        {
                            OptimizacionMueblesViewModel muebleImg = historico.OptimizacionMuebles.Select(x => x).Where(x => x.IdMueble == mueble.IdMueble).FirstOrDefault();

                            if (muebleImg != null && muebleImg.Imagen != null && muebleImg.Imagen.Length > 0)
                            {
                                string pathTempImage = pathImage + "\\temp " + muebleImg.IdMueble + ".jpg";

                                byte[] imageBytes = Convert.FromBase64String(muebleImg.Imagen.Split(',')[1]);
                                MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                                ms.Write(imageBytes, 0, imageBytes.Length);
                                System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);


                                int Width = (int)(mueble.VerticeDerechaArribaX - mueble.VerticeIzquierdaArribaX);
                                int Height = (int)(mueble.VerticeIzquierdaArribaY - mueble.VerticeIzquierdaAbajoY);
                                var newImage = ResizeImage(image, Width, Height);

                                if (!File.Exists(pathTempImage))
                                    newImage.Save(pathTempImage, ImageFormat.Jpeg);

                                optimizacionHistorialViewModel.PathsImages.Add(pathTempImage);

                                Vector3 vector3 = new Vector3();
                                vector3.Z = 0;
                                vector3.X =(double)mueble.VerticeIzquierdaAbajoX;
                                vector3.Y = (double)mueble.VerticeIzquierdaAbajoY;

                                netDxf.Objects.ImageDefinition imageDefinition = new netDxf.Objects.ImageDefinition(pathTempImage);
                                netDxf.Entities.Image imageToSave = new netDxf.Entities.Image(imageDefinition, vector3, imageDefinition.Width, imageDefinition.Height);

                                image.Dispose();
                                newImage.Dispose();
                                ms.Dispose();

                                dxfFinal.AddEntity(imageToSave);
                            }
                            else if(muebleImg != null)
                            {
                                List<LwPolylineVertex> verticesMueble = new List<LwPolylineVertex>();

                                Model.ViewModels.Vector2 verticeIzqueirdaArribaMueble = new Model.ViewModels.Vector2();
                                verticeIzqueirdaArribaMueble.X = (double)mueble.VerticeIzquierdaArribaX;
                                verticeIzqueirdaArribaMueble.Y = (double)mueble.VerticeIzquierdaArribaY;
                                verticesMueble.Add(mb.ConvertVertex(verticeIzqueirdaArribaMueble));

                                Model.ViewModels.Vector2 verticeDerechaArribaMueble = new Model.ViewModels.Vector2();
                                verticeDerechaArribaMueble.X = (double)mueble.VerticeDerechaArribaX;
                                verticeDerechaArribaMueble.Y = (double)mueble.VerticeDerechaArribaY;
                                verticesMueble.Add(mb.ConvertVertex(verticeDerechaArribaMueble));

                                Model.ViewModels.Vector2 verticeDerechaAbajoMueble = new Model.ViewModels.Vector2();
                                verticeDerechaAbajoMueble.X = (double)mueble.VerticeDerechaAbajoX;
                                verticeDerechaAbajoMueble.Y = (double)mueble.VerticeDerechaAbajoY;
                                verticesMueble.Add(mb.ConvertVertex(verticeDerechaAbajoMueble));

                                Model.ViewModels.Vector2 verticeIzqueirdaAbajoMueble = new Model.ViewModels.Vector2();
                                verticeIzqueirdaAbajoMueble.X = (double)mueble.VerticeIzquierdaAbajoX;
                                verticeIzqueirdaAbajoMueble.Y = (double)mueble.VerticeIzquierdaAbajoY;
                                verticesMueble.Add(mb.ConvertVertex(verticeIzqueirdaAbajoMueble));

                                dxfFinal.AddEntity(new LwPolyline(verticesMueble, true));
                            }
                        }
                    }

                    dxfFinal.Save(pathTemp);
                    dateTimeNow = dateTimeNow.AddSeconds(2);
                }

                optimizacionHistorialViewModel.PathsImages = optimizacionHistorialViewModel.PathsImages.Distinct().ToList();
                return optimizacionHistorialViewModel;
            }
        }

        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
