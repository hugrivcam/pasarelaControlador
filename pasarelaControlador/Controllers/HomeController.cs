using Microsoft.AspNetCore.Mvc;
using pasarelaControlador.Entidades;
using pasarelaControlador.Servicios;
namespace pasarelaControlador.Controllers

{
    [ApiController]
    [Route("api/[controller]")]  //api/HOME
    public class HomeController : Controller
    {
        private readonly ServicioControlProcesos _servicioControlProcesos;//injectamos el servicio singleton
        public HomeController(ServicioControlProcesos servicioControlProcesos)//en el constructor se inyectan los servicios, igual que en angular
        {
            _servicioControlProcesos = servicioControlProcesos; //ya tenemos el servicio injectado
        }

        [HttpGet]
        public async Task<ActionResult<List<DatosProceso>>> GetAllDatos() 
        {
            try
            {

                var datos = await _servicioControlProcesos.GetAllProcesos(); 
                return Ok(datos);//devuelve 200 ok con los datos obtenidos ya en formato JSON
            }
            catch (Exception ex) 
            {
                return StatusCode(500,"Se produjo un error al obtener los datos." + ex.Message);
            }
            //throw new NotImplementedException();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<DatosProceso>> GetProceso(long id)
        {
            //api/Home/{id}
            var datos = await _servicioControlProcesos.GetProceso(id);
            if (datos != null)
            {
                return Ok(datos);
            }
            else 
            {
                //return NotFound(); //devuelve codigo http 404
                //return StatusCode(500, "No se encontró el proceso con id " + id);
                return NotFound("No se encontró el proceso con id " + id);
            }
            //throw new NotImplementedException();
        }
        [HttpPost("{NombreProceso}")] //api/Home/{NombreProceso}
        public async Task<ActionResult<long>> CrearNuevoProceso(string nombreProceso) //sin id
        {
            try
            {
                CreateDatosProceso procesoNuevo = new CreateDatosProceso();
                procesoNuevo.Nombre = nombreProceso;
                procesoNuevo.estado = "Procesando";//esto podría comenzar parado y actualizarse a procesando más tarde
                long id = await _servicioControlProcesos.AddProceso(procesoNuevo);
                if (id > 0)
                {
                    return Ok(id);
                }
                else
                {
                    return BadRequest("No se pudo crear el nuevo proceso");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Se produjo un error al crear un nuevo proceso: " + ex.Message);
            }
            //throw new NotImplementedException();
        }



        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteDatos(long id) 
        {
            //api/Home/{id}
            try
            {
                var res = await _servicioControlProcesos.DeleteProceso(id);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Se produjo un error al eliminar datos. Id:" + id.ToString() + " ; " + ex.Message);
            }
            //throw new NotImplementedException();
        }


        [HttpPut("{id},{estado}")]
        public async Task<ActionResult<bool>> UpdateDatos(long id, string estado) //con id
        {
            var res = await _servicioControlProcesos.PutEstado(id, estado);
            if (res)
            {
                return Ok(res);
            }
            else
            {
                return NotFound("No se encontró el proceso con id " + id);
            }
                //throw new NotImplementedException();
        }


    }
}
