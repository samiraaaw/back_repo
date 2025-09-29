using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DICREP.EcommerceSubastas.Application.Validation;

namespace DICREP.EcommerceSubastas.Application.DTOs.FichaProducto
{
    public class FotografiaDto
    {
        [Required(ErrorMessage = "El url de la fotografía es requerido")]
        [RegularExpression(@"^https?:\/\/(?:www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b(?:[-a-zA-Z0-9()@:%_\+.~#?&\/=]*)$",
        ErrorMessage = "El URL de la fotografía no tiene un formato válido. Debe comenzar con http o https. Se permiten URLs con o " +
            "sin www., con un dominio de 1 a 256 caracteres y una extensión de 1 a 6 caracteres. También puede incluir rutas, parámetros y fragmentos opcionales. ")]
        public string url { get; set; }


        [Required(ErrorMessage = "El tipo de formato de la fotografía es requerido")]
        [RegularExpression(@"^\.?(jpg|jpeg|png)$", ErrorMessage = "Formato de fotografía no válido. Solo se permiten archivos con extensión jpg, jpeg o png")]
        public string formato { get; set; }
    }


    public class InformeTecnicoDto
    {

        [Required(ErrorMessage = "El url del informe técnico es requerido")]
        [Url(ErrorMessage = "La url del informe técnico no tiene un formato válido")]
        [RegularExpression(@"^https?:\/\/(?:www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b(?:[-a-zA-Z0-9()@:%_\+.~#?&\/=]*)$",
        ErrorMessage = "El URL del informe técnico no tiene un formato válido. Debe comenzar con http o https. Se permiten URLs con o " +
            "sin www., con un dominio de 1 a 256 caracteres y una extensión de 1 a 6 caracteres. También puede incluir rutas, parámetros y fragmentos opcionales. ")]
        public string url { get; set; }

        [Required(ErrorMessage = "El tipo de formato del informe técnico es requerido")]
        [RegularExpression(@"^\.?(pdf|docx)$", ErrorMessage = "Formato de informe técnico no válido. Solo se permiten archivos con extensión pdf o docx")]
        public string tipo { get; set; }
    }





    public class ContactoOrganismoDto
    {
        [Required(ErrorMessage = "El id del organismo es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El id del Organismo debe ser un número mayor o igual a 1.")]
        public int? idOrganismo { get; set; } 

        [Required(ErrorMessage = "El rut del organismo es requerido")]
        [RutValid(ErrorMessage = "El RUT ingresado no es válido. Debe ser un RUT chileno correcto, escrito con puntos, guion y su dígito verificador")] 
        public string rut { get; set; }

        [Required(ErrorMessage = "El Nombre del Organismo es requerido")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "La longitud del nombre del organismo debe estar entre 5 y 250 caracteres.")]
        public string nombre { get; set; }


        [Required(ErrorMessage = "El correo electrónico del organismo es requerido")]
        [RegularExpression(@"^(?=.{1,100}$)[a-zA-Z0-9._%+-]+@(?=.{1,100}$)([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$", ErrorMessage = "Por favor, introduce una dirección de correo electrónico válida.El correo electrónico no tiene un formato válido. Debe contener un nombre de usuario con letras, números y los caracteres permitidos (., _, %, +, -), \" +\r\n            \"seguido del símbolo @ obligatorio, un dominio que puede incluir letras, números, puntos o guiones, y un punto antes de la extensión, la cual debe tener al menos 2 caracteres.")]
        public string correo { get; set; }

        [Required(ErrorMessage = "El teléfono del Organismo es requerido")]
        [RegularExpression(@"^(\+?56)?\s?(\d{1,2}\s?\d{3}\s?\d{4}|\d{8,9})$",
            ErrorMessage = "El teléfono debe ser chileno (fijo o celular). Formato válido: +56 9 5714 8746, +56 2 1234 5678, 32 123 4567, 912345678, etc.")]
        public string telefono { get; set; }
    }


    public class DireccionOrganismoDto
    {
        [Required(ErrorMessage = "El id de la comuna es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El id de la comuna debe ser un número mayor o igual a 1.")]
        public int? IdComuna { get; set; }

        [Required(ErrorMessage = "El nombre de la comuna es requerido")]
        [StringLength(100, ErrorMessage = "El nombre de la comuna debe tener entre 1 y 100 caracteres.")]
        [RegularExpression(@"^[A-Za-zÀ-ÿ]+(?:\s+[A-Za-zÀ-ÿ]+)*$",
    ErrorMessage = "Nombre de comuna inválido. Por favor, ingresa solo letras (incluyendo acentos y la letra ñ) y espacios.")]
        public string comuna { get; set; }


        [Required(ErrorMessage = "La dirección del organismo es requerida")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "La dirección del organismo debe tener entre 1 y 255 caracteres.")]
        [RegularExpression(@"^[A-Za-zÀ-ÿ0-9\s.,\-#]+$",
    ErrorMessage = "Dirección inválida. Por favor, ingresa solo letras, números, espacios y los símbolos permitidos (como '.', ',', '-', '#').")]
        public string direccion { get; set; }

    }

    public class DetalleBienDto
    {
        [Required(ErrorMessage = "El id de publicación del bien es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El id del bien debe ser un número mayor o igual a 1.")]
        public string id_publicacion_bien { get; set; }

        [Required(ErrorMessage = "El id de la categoria del bien es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El id de la categoría debe ser un número mayor o igual a 1.")]
        public int? idCategoria { get; set; }

        [Required(ErrorMessage = "El nombre de la categoria del bien es requerido")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "La categoría debe tener entre 1 y 255 caracteres.")]
        public string categoria { get; set; }


        [Required(ErrorMessage = "El nombre del bien es requerido")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "El nombre del bien debe tener entre 2 y 255 caracteres.")]
        public string nombre { get; set; }


        [Required(ErrorMessage = "La descripcion del bien es requerida")]
        [StringLength(8000, MinimumLength = 2, ErrorMessage = "La descripción del bien debe tener entre 2 y 1000 caracteres.")] //preguntar si cambiar a minimo 10
        public string descripcion { get; set; }
        public string tamano { get; set; }
        public string color { get; set; }
        

        [Required(ErrorMessage = "La cantidad del bien es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 1")]
        public int cantidad { get; set; }

        public string tipo_cantidad { get; set; }

        [Required(ErrorMessage = "El valor estimado del bien es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El valor estimado del bien debe ser mínimo de $1")]
        public decimal valor_estimado { get; set; }

        [Required(ErrorMessage = "El id del estado del bien es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El id del estado del bien debe ser un número mayor o igual a 1.")]
        public int? IdEstado { get; set; } 

        [Required(ErrorMessage = "El nombre del estado del Bien es requerido")]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "El nombre del estado debe tener entre 1 y 30 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "El nombre del estado solo puede contener letras, números y espacios.")]
        public string estado { get; set; }
        public bool Desmontable { get; set; }
        public bool BajaToxicidad { get; set; }


        [Required(ErrorMessage = "El informe técnico es requerido")]
        [MinLength(1, ErrorMessage = "Debe ingresar al menos un informe técnico.")]
        [MaxLength(6, ErrorMessage = "No puede ingresar más de 6 informes técnicos.")]
        public List<InformeTecnicoDto> informes_tecnicos { get; set; }


        [Required(ErrorMessage = "El contacto del organismo es requerido")]
        public ContactoOrganismoDto contacto_organismo { get; set; }

        [Required(ErrorMessage = "La dirección del organismo es requerida")]
        public DireccionOrganismoDto direccion_organismo { get; set; }
    }

    public class FichaProductoDto
    {
        [Required(ErrorMessage = "Debe ingresar al menos una fotografía")]
        [MinLength(3, ErrorMessage = "Debe ingresar al menos 3 fotografías.")]
        [MaxLength(6, ErrorMessage = "No puede ingresar más de 6 fotografías")]
        public List<FotografiaDto> fotografias { get; set; }

        [Required(ErrorMessage = "El detalle del bien es requerido")]
        public DetalleBienDto detalle_bien { get; set; }
    }

    public class ReceiveFichaDto
    {
        [Required(ErrorMessage = "La ficha del producto es obligatoria")]
        public FichaProductoDto ficha_producto { get; set; }
    }

}
