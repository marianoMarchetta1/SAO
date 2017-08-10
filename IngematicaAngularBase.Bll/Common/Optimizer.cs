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

            //List<Circle> ciruclos = initialFlat.Circles.ToList();

            /*    Circle circulo = new Circle();
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

                initialFlat.AddEntity(linea);*/

            //Rectangle celda = GetTamanioCelda();            Obtiene el tamaño que se usara para particionar el plano.

            //return initialFlat;

            return null;
        }
    }
}
