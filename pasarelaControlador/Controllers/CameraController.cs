using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pasarelaControlador.Entidades;
using pasarelaControlador.EntidadesJSON;
using pasarelaControlador.Servicios;
using System.ComponentModel;

namespace pasarelaControlador.Controllers
{
    //pendiente de implementar
    [ApiController]//imeplementando controlador API REST
    [Route("[controller]")]
    public class CameraController : Controller
    {
        //primero injectar servicio o servicios que vamos a usar, en este caso el de la camara
        private readonly ServicioCamara _servicioCamara;
        public CameraController(ServicioCamara servicioCamara)
        {
            _servicioCamara = servicioCamara;//servicio inyectado :)
        }

        [HttpGet("ListaCamaras")]
        public async Task<ActionResult<List<CamaraJSON>>> GetListCamarasObj()
        {
            List<CamaraJSON> l = await Task.Run(() => _servicioCamara.GetListaCamarasObj());//convierto la tarea en asincrona
            return Ok(l);
        }
        //escojer y encender camara, devolver ok si no hay error
        [HttpGet("SetCamera/{id}")]
        public async Task<ActionResult<CamaraJSON>> SetCamera(int id)
        {
            var res = await Task.Run(() => _servicioCamara.EncenderCamara(id));//convierto la tarea en asincrona
            if (res.Id >=0)
                return Ok(res); //status code 200
            else
                return BadRequest(res); //status code 400
        }
        //hacer foto y devolver id_foto
        [HttpGet("MakePhoto")]
        public async Task<ActionResult<FotoJSON>> MakePhoto()
        {
            var res = await Task.Run(() => _servicioCamara.HacerFoto());
            if (res >= 0)
            {
                var fotoJSON = _servicioCamara.GetLastFotoJSON();
                return Ok(fotoJSON); //-1 si no hay foto, si no devuelve el indice de la foto
            }
            else
            {
                FotoJSON fotoJSON = new();
                fotoJSON.Id = -1;
                fotoJSON.FileName = "No se pudo realizar la foto";
                fotoJSON.Date = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                return BadRequest(fotoJSON);
            }
            
        }
  
        //enviar foto streaming segun el id indicado de la camara
        [HttpGet("StreamPhoto/{idFoto}")]
        public IActionResult StreamPhoto(int idFoto)
        {
            //Vamos a usar el metodo File() que implementa la interfaz IActrionResult devolviendo un FileResult que se puede enviar  mediante API/Rest
            try
            {
                var foto = _servicioCamara.GetFoto(idFoto);
                if (foto != null)
                {
                    //string filePath = foto.RutaFile!;
                    //creamos un stream de la lectura del archivo
                    if (System.IO.File.Exists(foto.RutaFile))
                    {
                        var fileStream = new FileStream(foto.RutaFile!, FileMode.Open, FileAccess.Read, FileShare.Read);
                        return File(fileStream, "image/jpeg");//al terminar el flujo de datos el archivo se cierra automáticamente
                    }
                    else
                        return NotFound("Archivo no encontrado: " + foto.RutaFile!);
                }
                else
                {
                    return NotFound("id foto no encontrada: " + idFoto); // 404 not found // BadRequest("No se encontró la foto con id " + id);
                }
            }
            catch (Exception ex) 
            {
                return BadRequest("Error en API en StreamPhoto: " + ex.Message);
            }
        }

        [HttpGet("ApagarCamara")]
        public async Task<ActionResult<bool>> ApagaCamara() 
        {
            bool res = await _servicioCamara.ApagarCamaraActual();
            if (res)
                return Ok(res);
            else
                return BadRequest("No se pudo apagar la camara:" + res);//este codigo nunca se ejecuta, tarde más o tarde menos debería apagarse en cualquier caso sera el cliente el que quizá de la por apagada si la camara no respodne.
        }
    }
}
