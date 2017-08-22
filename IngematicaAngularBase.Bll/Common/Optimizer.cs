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

                    while (muebleListTemp.Count > 0 && hayEspacio)
                    {
                        celda = GetTamañoMaximoCelda(muebleList, (int)sentido);
                        //ubicar muebles(vertices, muebleList, sentido) -> Eliminarlos de muebleListTemp
                        //hayEspacio = Compactar(vertices, muebleList, sentido) -> Compacta la lista de muebles y retorna si queda lugar libre
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


        public decimal GetAnchoPasillo(LwPolyline lwPolyline, int sentido)
        {
            return 0;
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
