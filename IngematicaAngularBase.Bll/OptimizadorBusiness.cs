﻿using IngematicaAngularBase.Dal;
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
using IngematicaAngularBase.Bll.Common;

namespace IngematicaAngularBase.Bll
{
    public class OptimizadorBusiness
    {
        public List<SelectionListSimple> GetMuebleList(bool activo)
        {
            using (var context = new Entities())
            {
                OptimizadorDataAccess optimizadorDataAccess = new OptimizadorDataAccess(context);
                return optimizadorDataAccess.GetMuebleList(activo);
            }
        }

        public string Generate(OptimizadorOptimizacionViewModel file)
        {
            using (var context = new Entities())
            {
                MuebleDataAccess muebleDataAcces = new MuebleDataAccess(context);

                DxfDocument dxfInitial = DxfDocument.Load(file.Archivo.Path);

                Optimizer optimizer = new Optimizer(dxfInitial, 
                                                    file.OptimizarCosto, 
                                                    file.CostoMaximo, 
                                                    file.MuebleList, 
                                                    muebleDataAcces.GetMuebleList(file.MuebleList.Select(x=> x.IdMueble).ToList()),
                                                    file.Escala);

                DxfDocument dxfFinal = optimizer.Generate();

                string path = System.Configuration.ConfigurationManager.AppSettings["TmpFiles"];
                dxfFinal.Save(path);

                //if(file.RegistrarEnHistorial){
                //  OptimizadorDataAccess optimizadorDataAccess = new OptimizadorDataAccess(Entities);
                //  optimizadorDataAccess.AddHistory(dxfFinal);         -> almacena en un tabla de la base el "historial" que seria la lista de entidades del plano
                //}

                return path;
            }
        }
    }
}