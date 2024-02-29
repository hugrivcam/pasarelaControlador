namespace pasarelaControlador.Entidades
{
    public class Foto
    {
        public int Id { get; set; } //cada dia empieza desde 1
        public string? RutaFile { get; set; } //ruta completa con el nombre del fichero
        public string? FileName { get; set; } 
        public string? Ruta {  get; set; } //ruta base donde se guardan las fotos
        public DateTime? Date { get; set; } //fecha_hora_en_la_que_se_crea_el_archivo
    }
}
