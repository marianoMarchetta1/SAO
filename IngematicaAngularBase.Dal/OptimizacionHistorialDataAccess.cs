using IngematicaAngularBase.Dal.Specification;
using IngematicaAngularBase.Model.Common;
using IngematicaAngularBase.Model.Entities;
using IngematicaAngularBase.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngematicaAngularBase.Dal
{
    public class OptimizacionHistorialDataAccess
    {
        public OptimizacionHistorialDataAccess(Entities context)
        {
            this.context = context;
        }

        private readonly Entities context;

        public QueryResult<OptimizacionHistorialListViewModel> GetList(OptimizacionHistorialQuery query)
        {
            IQueryable<OptimizacionHistorial> tOptimizacionHistorial = context.Set<OptimizacionHistorial>().AsNoTracking();
            tOptimizacionHistorial = tOptimizacionHistorial.WithContainsNombre(query.Nombre);

            var result = from optimizacionHistorial in tOptimizacionHistorial
                         select new OptimizacionHistorialListViewModel
                         {
                             CantidadPersonas = optimizacionHistorial.CantidadPersonas,
                             Nombre = optimizacionHistorial.Nombre,
                             CostoMaximo = optimizacionHistorial.CostoMaximo,
                             Escala = optimizacionHistorial.Escala,
                             IdOptimizacionHistorial = optimizacionHistorial.IdOptimizacionHistorial
                         };
            QueryResult<OptimizacionHistorialListViewModel> rta = result.ToQueryResult(query);
            rta.Data = rta.Data.GroupBy(x => x.Nombre).Select(x=> x.First()).ToList();
            return rta;
        }

        public OptimizacionHistorialViewModel GetById(int id)
        {
            IQueryable<OptimizacionHistorial> tOptimizacionHistorial = context.Set<OptimizacionHistorial>().AsNoTracking();
            IQueryable<OptimizacionHistorialArea> tOptimizacionHistorialArea = context.Set<OptimizacionHistorialArea>().AsNoTracking();
            IQueryable<OptimizacionHistorialAreaMueble> tOptimizacionHistorialAreaMueble = context.Set<OptimizacionHistorialAreaMueble>().AsNoTracking();


            var result = from optimizacionHistorial in tOptimizacionHistorial
                         where optimizacionHistorial.IdOptimizacionHistorial == id
                         select optimizacionHistorial;

            OptimizacionHistorial optHist = result.First();

            result = from optimizacionHistorial in tOptimizacionHistorial
                     where optimizacionHistorial.Nombre == optHist.Nombre
                     select optimizacionHistorial;

            List<int> idsOptimizacionHistorial = result.Select(x => x.IdOptimizacionHistorial).ToList();

            OptimizacionHistorialViewModel rta = new OptimizacionHistorialViewModel();
            rta.Nombre = optHist.Nombre;
            rta.OptimizarCosto = optHist.OptimizarCosto == null ? false : (bool)optHist.OptimizarCosto;
            rta.Escala = optHist.Escala;
            rta.CostoMaximo = optHist.CostoMaximo == null ? 0 : (double) optHist.CostoMaximo;
            rta.CantidadPersonas = optHist.CantidadPersonas;
            rta.Paths = new List<string>();

            var resultOptimizacionArea = from optimizacionHistorialArea in tOptimizacionHistorialArea
                                         where idsOptimizacionHistorial.Contains(optimizacionHistorialArea.IdOptimizacionHistorial)
                                         select new OptimizacionHistorialAreaViewModel {
                                             IdOptimizacionHistorial = optimizacionHistorialArea.IdOptimizacionHistorial,
                                             IdOptimizacionHistorialArea = optimizacionHistorialArea.IdOptimizacionHistorialArea,
                                             VerticeDerechaAbajoX = optimizacionHistorialArea.VerticeDerechaAbajoX,
                                             VerticeDerechaAbajoY = optimizacionHistorialArea.VerticeDerechaAbajoY,
                                             VerticeDerechaArribaX = optimizacionHistorialArea.VerticeDerechaArribaX,
                                             VerticeDerechaArribaY = optimizacionHistorialArea.VerticeDerechaArribaY,
                                             VerticeIzquierdaAbajoX = optimizacionHistorialArea.VerticeIzquierdaAbajoX,
                                             VerticeIzquierdaAbajoY = optimizacionHistorialArea.VerticeIzquierdaAbajoY,
                                             VerticeIzquierdaArribaX = optimizacionHistorialArea.VerticeIzquierdaArribaX,
                                             VerticeIzquierdaArribaY = optimizacionHistorialArea.VerticeIzquierdaArribaY
                                         };

            rta.OptimizacionHistorialArea = resultOptimizacionArea.ToList();

            foreach(OptimizacionHistorialAreaViewModel optimizacionHistorialArea in rta.OptimizacionHistorialArea)
            {
                optimizacionHistorialArea.OptimizacionHistorialAreaMueble = new List<OptimizacionHistorialAreaMuebleViewModel>();
                tOptimizacionHistorialAreaMueble = context.Set<OptimizacionHistorialAreaMueble>().AsNoTracking();

                var resultOptimizacionHistorialAreaMueble = from optimizacionHistorialAreaMueble in tOptimizacionHistorialAreaMueble
                                                            where optimizacionHistorialAreaMueble.IdOptimizacionHistorialArea == optimizacionHistorialArea.IdOptimizacionHistorialArea
                                                            select new OptimizacionHistorialAreaMuebleViewModel()
                                                            {
                                                                IdOptimizacionHistorailAreaMueble = optimizacionHistorialAreaMueble.IdOptimizacionHistorialAreaMueble,
                                                                IdOptimizacionHistorialArea = optimizacionHistorialAreaMueble.IdOptimizacionHistorialArea,
                                                                VerticeDerechaAbajoX = optimizacionHistorialAreaMueble.VerticeDerechaAbajoX,
                                                                VerticeDerechaAbajoY = optimizacionHistorialAreaMueble.VerticeDerechaAbajoY,
                                                                VerticeDerechaArribaX = optimizacionHistorialAreaMueble.VerticeDerechaArribaX,
                                                                VerticeDerechaArribaY = optimizacionHistorialAreaMueble.VerticeDerechaArribaY,
                                                                VerticeIzquierdaAbajoX = optimizacionHistorialAreaMueble.VerticeIzquierdaAbajoX,
                                                                VerticeIzquierdaAbajoY = optimizacionHistorialAreaMueble.VerticeIzquierdaAbajoY,
                                                                VerticeIzquierdaArribaX = optimizacionHistorialAreaMueble.VerticeIzquierdaArribaX,
                                                                VerticeIzquierdaArribaY = optimizacionHistorialAreaMueble.VerticeIzquierdaArribaY
                                                            };

                optimizacionHistorialArea.OptimizacionHistorialAreaMueble = resultOptimizacionHistorialAreaMueble.ToList();

            }

            tOptimizacionHistorial = context.Set<OptimizacionHistorial>().AsNoTracking();
            IQueryable<OptimizacionMuebles> tOptimizacionMuebles = context.Set<OptimizacionMuebles>().AsNoTracking();

            var mueblesResult = from optimizacionHistorial in tOptimizacionHistorial
                                join optimizacionMuebles in tOptimizacionMuebles on optimizacionHistorial.IdOptimizacionHistorial equals optimizacionMuebles.IdOptimizacionHistorial
                                select new OptimizacionMueblesViewModel()
                                {
                                    Cantidad = optimizacionMuebles.Cantidad,
                                    Mueble = optimizacionMuebles.Mueble
                                };

            rta.OptimizacionMuebles = mueblesResult.Distinct().ToList();

            return rta;
        }


        public List<OptimizacionHistorialViewModel> GetByIdSinAgrupar(int id)
        {
            IQueryable<OptimizacionHistorial> tOptimizacionHistorial = context.Set<OptimizacionHistorial>().AsNoTracking();
            IQueryable<OptimizacionHistorialArea> tOptimizacionHistorialArea = context.Set<OptimizacionHistorialArea>().AsNoTracking();
            IQueryable<OptimizacionHistorialAreaMueble> tOptimizacionHistorialAreaMueble = context.Set<OptimizacionHistorialAreaMueble>().AsNoTracking();


            var result = from optimizacionHistorial in tOptimizacionHistorial
                         where optimizacionHistorial.IdOptimizacionHistorial == id
                         select optimizacionHistorial;

            OptimizacionHistorial optHist = result.First();

            result = from optimizacionHistorial in tOptimizacionHistorial
                     where optimizacionHistorial.Nombre == optHist.Nombre
                     select optimizacionHistorial;

            List<OptimizacionHistorial> optimizacionHistorialList = result.ToList();

            List<OptimizacionHistorialViewModel> rta = new List<OptimizacionHistorialViewModel>();

            foreach(OptimizacionHistorial opt in optimizacionHistorialList)
            {
                OptimizacionHistorialViewModel optHistVm = new OptimizacionHistorialViewModel();
                optHistVm.IdOptimizacionHistorial = opt.IdOptimizacionHistorial;
                optHistVm.Paths = new List<string>();
                optHistVm.OptimizacionHistorialArea = new List<OptimizacionHistorialAreaViewModel>();

                rta.Add(optHistVm);

                tOptimizacionHistorialArea = context.Set<OptimizacionHistorialArea>().AsNoTracking();

                var resultOptimizacionArea = from optimizacionHistorialArea in tOptimizacionHistorialArea
                                             where optimizacionHistorialArea.IdOptimizacionHistorial == opt.IdOptimizacionHistorial
                                             select new OptimizacionHistorialAreaViewModel
                                             {
                                                 IdOptimizacionHistorial = optimizacionHistorialArea.IdOptimizacionHistorial,
                                                 IdOptimizacionHistorialArea = optimizacionHistorialArea.IdOptimizacionHistorialArea,
                                                 VerticeDerechaAbajoX = optimizacionHistorialArea.VerticeDerechaAbajoX,
                                                 VerticeDerechaAbajoY = optimizacionHistorialArea.VerticeDerechaAbajoY,
                                                 VerticeDerechaArribaX = optimizacionHistorialArea.VerticeDerechaArribaX,
                                                 VerticeDerechaArribaY = optimizacionHistorialArea.VerticeDerechaArribaY,
                                                 VerticeIzquierdaAbajoX = optimizacionHistorialArea.VerticeIzquierdaAbajoX,
                                                 VerticeIzquierdaAbajoY = optimizacionHistorialArea.VerticeIzquierdaAbajoY,
                                                 VerticeIzquierdaArribaX = optimizacionHistorialArea.VerticeIzquierdaArribaX,
                                                 VerticeIzquierdaArribaY = optimizacionHistorialArea.VerticeIzquierdaArribaY
                                             };

                optHistVm.OptimizacionHistorialArea = resultOptimizacionArea.ToList();

                foreach(OptimizacionHistorialAreaViewModel optimizacionHistorialAreaViewModel in optHistVm.OptimizacionHistorialArea)
                {
                    optimizacionHistorialAreaViewModel.OptimizacionHistorialAreaMueble = new List<OptimizacionHistorialAreaMuebleViewModel>();

                    tOptimizacionHistorialAreaMueble = context.Set<OptimizacionHistorialAreaMueble>().AsNoTracking();

                    var resultOptimizacionHistorialAreaMueble = from optimizacionHistorialAreaMueble in tOptimizacionHistorialAreaMueble
                                                                where optimizacionHistorialAreaMueble.IdOptimizacionHistorialArea == optimizacionHistorialAreaViewModel.IdOptimizacionHistorialArea
                                                                select new OptimizacionHistorialAreaMuebleViewModel()
                                                                {
                                                                    IdOptimizacionHistorailAreaMueble = optimizacionHistorialAreaMueble.IdOptimizacionHistorialAreaMueble,
                                                                    IdOptimizacionHistorialArea = optimizacionHistorialAreaMueble.IdOptimizacionHistorialArea,
                                                                    VerticeDerechaAbajoX = optimizacionHistorialAreaMueble.VerticeDerechaAbajoX,
                                                                    VerticeDerechaAbajoY = optimizacionHistorialAreaMueble.VerticeDerechaAbajoY,
                                                                    VerticeDerechaArribaX = optimizacionHistorialAreaMueble.VerticeDerechaArribaX,
                                                                    VerticeDerechaArribaY = optimizacionHistorialAreaMueble.VerticeDerechaArribaY,
                                                                    VerticeIzquierdaAbajoX = optimizacionHistorialAreaMueble.VerticeIzquierdaAbajoX,
                                                                    VerticeIzquierdaAbajoY = optimizacionHistorialAreaMueble.VerticeIzquierdaAbajoY,
                                                                    VerticeIzquierdaArribaX = optimizacionHistorialAreaMueble.VerticeIzquierdaArribaX,
                                                                    VerticeIzquierdaArribaY = optimizacionHistorialAreaMueble.VerticeIzquierdaArribaY
                                                                };

                    optimizacionHistorialAreaViewModel.OptimizacionHistorialAreaMueble = resultOptimizacionHistorialAreaMueble.ToList();

                }
            }

            return rta;
        }
    }
}
    