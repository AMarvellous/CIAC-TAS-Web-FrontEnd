namespace CIAC_TAS_Web_UI.ModelViews.Instructores
{
    public class InstructorProgramaAnaliticoModelView
    {
        public int InstructorId { get; set; }
        public string InstructorNombre { get; set; }
        public int ProgramaAnaliticoPdfId { get; set; }
        public string ProgramaAnaliticoPdfRuta { get; set; }
        public int ProgramaAnaliticoPdfMateriaId { get; set; }
        public string ProgramaAnaliticoPdfMateriaNombre { get; set; }
        public int ProgramaAnaliticoPdfGestion { get; set; }
    }
}
