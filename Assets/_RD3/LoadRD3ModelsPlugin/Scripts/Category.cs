namespace _RD3.LoadRD3ModelsPlugin.Scripts
{
    [System.Flags]
    public enum Category
    {
        None = 0,
        Mobiliário = 1 << 0, 
        Veículos = 1 << 1,    
        Animais = 1 << 2,     
        Personagens = 1 << 3,
        Vegetação = 1 << 4,   
        ObjetosInternos = 1 << 5, 
        ObjetosExternos = 1 << 6, 
        Produtos = 1 << 7,    
        Animações = 1 << 8,   
        Arquitetura = 1 << 9, 
        Comida = 1 << 10,     
        Industrial = 1 << 11, 
        Outros = 1 << 12,     
        Todos = ~0            
    }
}