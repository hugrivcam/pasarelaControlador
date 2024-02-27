using pasarelaControlador.Entidades;

namespace pasarelaControlador.Servicios
{
    public class ServicioControlProcesos
    {
        private readonly List<DatosProceso> ListaProcesos = new List<DatosProceso>();
        public ServicioControlProcesos() 
        {
            //por defecto creamos un proceso inicial
            this.CrearNuevoProcesoInicial();
        }
        public async Task<List<DatosProceso>> GetAllProcesos() 
        {
                return await Task.FromResult(ListaProcesos);
        }
        public async Task<DatosProceso?> GetProceso(long id) //ojo puede devolver null
        {
            DatosProceso? procesoEncontrado = ListaProcesos.Find(cadaProceso => cadaProceso.Id == id);
            return await Task.FromResult(procesoEncontrado);
        }
        public async Task<long> AddProceso(CreateDatosProceso nuevoProcesoCreado)
        {
            DatosProceso nuevoProceso = new DatosProceso();
            long id = ListaProcesos.Count + 1;
            MapNuevoProceso(nuevoProcesoCreado, nuevoProceso, id);
            ListaProcesos.Add(nuevoProceso);
            return await Task.FromResult(id);
        }
        public async Task<bool> PutEstado(long id, string estado)
        {
            //ListaProcesos[id - 1].estado = estado;
            DatosProceso procesoEncontrado = ListaProcesos.Find(cadaProceso => cadaProceso.Id == id);
            if (procesoEncontrado != null) //si encontramos el proceso
            {
                procesoEncontrado.estado = estado;
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false); ;
                //throw new ArgumentException("No se encontró ningún proceso con el ID especificado." + id);
            }  
        }
        public async Task<bool> DeleteProceso(long id) 
        {
            if (ListaProcesos == null)
                return await Task.FromResult(false);
            else
            {
                DatosProceso? procesoEncontrado = ListaProcesos.Find(cadaProceso => cadaProceso.Id == id); //con el interrogante le indico que puede devolver null
                if (procesoEncontrado != null)
                {
                    ListaProcesos.Remove(procesoEncontrado); ;
                    return await Task.FromResult(true);
                }
                else
                {
                    return await Task.FromResult(false);
                }
            }
        }
        public async Task<long> FindByName(string nombre) 
        {

            DatosProceso? procesoEncontrado = ListaProcesos!.Find(cadaProceso => cadaProceso.Nombre == nombre);
            if (procesoEncontrado != null) 
            {
                return await Task.FromResult(procesoEncontrado.Id);
            }
            return await Task.FromResult(-1L);
        }
        void MapNuevoProceso(CreateDatosProceso origen, DatosProceso destino, long nuevoID)
        {
            destino.Id = nuevoID;
            destino.Nombre = origen.Nombre;
            //destino.Accion = origen.Accion;//ultima accion recibida
            destino.estado = origen.estado;  //procesando / parado
        }
        public async Task<int> CrearNuevoProcesoInicial()
        {
            CreateDatosProceso np = new();
            np.Nombre = "Proceso Camara 1";
            np.estado = "Stop";
            //Stop / Working
            await this.AddProceso(np);
            return 1;
        }

    }
}
