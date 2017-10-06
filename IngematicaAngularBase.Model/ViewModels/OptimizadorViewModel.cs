using IngematicaAngularBase.Model;
using IngematicaAngularBase.Model.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace IngematicaAngularBase.Model.ViewModels
{
    public class OptimizadorOptimizacionViewModel
    {
        public string Nombre { get; set; }

        public bool RegistrarEnHistorial { get; set; }
        public bool OptimizarCosto { get; set; }

        public double CostoMaximo { get; set; }

        public List<OptimizacionMueble> MuebleList { get; set; }

        public PlanoData Archivo { get; set; } 

        public int? CantidadPersonas { get; set; }

        public string Escala { get; set; }
    }

    public class OptimizacionMueble
    {
        //public int IdOptimizacionMueble { get; set; }
        public int IdMueble { get; set; }
        public int Cantidad { get; set; }
    }

    public class PlanoData
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
    }

    public class Celda
    {
        public double Largo { get; set; }
        public double Ancho { get; set; }
    }

    public class AreaOptimizacion
    {
        public Vector2 VerticeIzquierdaArriba { get; set; }
        public Vector2 VerticeDerechaArriba { get; set; }

        public Vector2 VerticeIzquierdaAbajo { get; set; }
        public Vector2 VerticeDerechaAbajo { get; set; }

        public double Area { get; set; }

        public double Largo { get; set; }
        public double Ancho { get; set; }

        public List<MueblesOptmizacion> MueblesList { get; set; }
    }

    public class MueblesOptmizacion
    {
        public Mueble Mueble { get; set; }

        public Vector2 VerticeIzquierdaArriba { get; set; }
        public Vector2 VerticeDerechaArriba { get; set; }

        public Vector2 VerticeIzquierdaAbajo { get; set; }
        public Vector2 VerticeDerechaAbajo { get; set; }

        public double Largo { get; set; }
        public double Ancho { get; set; }
        public double Area { get; set; }

        public double? VerticeIzquierdaArribaX { get; set; }
        public double? VerticeIzquierdaArribaY { get; set; }

        public double? VerticeDerechaArribaX { get; set; }
        public double? VerticeDerechaArribaY { get; set; }

        public double? VerticeIzquierdaAbajoX { get; set; }
        public double? VerticeIzquierdaAbajoY { get; set; }

        public double? VerticeDerechaAbajoX { get; set; }
        public double? VerticeDerechaAbajoY { get; set; }
    }

    public class OptimizacionHistorialViewModel
    {
        public int IdOptimizacionHistorial { get; set; }
        public string Nombre { get; set; }
        public int? CantidadPersonas { get; set; }
        public string Escala { get; set; }
        public bool OptimizarCosto { get; set; }
        public double CostoMaximo { get; set; }

        public List<OptimizacionHistorialAreaViewModel> OptimizacionHistorialArea { get; set; }
        public List<OptimizacionMueblesViewModel> OptimizacionMuebles { get; set; }

        public List<string> Paths { get; set; }
    }

    public class OptimizacionMueblesViewModel{
        public int Cantidad { get; set; }
        public string Mueble { get; set; }
        public int IdOptimizacionHistorial { get; set; }
        public int IdOptimizacionMuebles { get; set; }
    }

    public class OptimizacionHistorialAreaViewModel
    {
        public int IdOptimizacionHistorialArea { get; set; }
        public int IdOptimizacionHistorial { get; set; }

        public double? VerticeIzquierdaArribaX { get; set; }
        public double? VerticeIzquierdaArribaY { get; set; }

        public double? VerticeDerechaArribaX { get; set; }
        public double? VerticeDerechaArribaY { get; set; }

        public double? VerticeIzquierdaAbajoX { get; set; }
        public double? VerticeIzquierdaAbajoY { get; set; }

        public double? VerticeDerechaAbajoX { get; set; }
        public double? VerticeDerechaAbajoY { get; set; }

        public List<OptimizacionHistorialAreaMuebleViewModel> OptimizacionHistorialAreaMueble { get; set; }
    }

    public class OptimizacionHistorialAreaMuebleViewModel
    {
        public int IdOptimizacionHistorailAreaMueble { get; set; }
        public int IdOptimizacionHistorialArea { get; set; }

        public double? VerticeIzquierdaArribaX { get; set; }
        public double? VerticeIzquierdaArribaY { get; set; }

        public double? VerticeDerechaArribaX { get; set; }
        public double? VerticeDerechaArribaY { get; set; }

        public double? VerticeIzquierdaAbajoX { get; set; }
        public double? VerticeIzquierdaAbajoY { get; set; }

        public double? VerticeDerechaAbajoX { get; set; }
        public double? VerticeDerechaAbajoY { get; set; }

        //public int IdMueble { get; set; }
    }

    public class Vector2
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public enum SentidoPasillosEnum
    {
        Largo = 1,
        Ancho = 2/*,
        Cruzado = 3*/
    }
}
