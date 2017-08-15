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

        private List<Circle> circles;
        private List<Line> lines;
        //Lista completa de las entidades a utilizar

        public Optimizer(DxfDocument dxfDocument, bool optimizarCostoParam, decimal costoMaximoParam, List<OptimizacionMueble> muebleCantidadListParam, List<Mueble> muebleListParam, string escalaParam)
        {
            this.initialFlat = dxfDocument;
            this.optimizarCosto = optimizarCostoParam;
            this.costoMaximo = costoMaximoParam;
            this.muebleCantidadList = muebleCantidadListParam;
            this.muebleList = muebleListParam;
            this.escala = escalaParam;
        }

        public DxfDocument Generate()
        {

            //validar cantidad de personas por metro cuadrado. Si hay de mas => lanzo excepcion

            //Definir ubicacion de los pasillos

            //Mientras haya lugar en el plano o muebles para cargar{

                //Obtener tamaño de la celda    (1)

                //Realizar la carga de muebles. Mientras haya espacio. Considerando la prioridad de cada mueble.    (2) Funcion que recibe limites en X e Y libres, inicialmente todo el plano.

                //Correr Compactacion. Retorna los limites de X e Y ya ocupados luego de la compaction.     (3)
            //}

            return null;
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
