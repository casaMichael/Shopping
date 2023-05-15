﻿using Shopping.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Shopping.Models
{
    public class ShowCartViewModel
    {
        //Que mostraremos en el carrito de compras
        public User User { get; set; }

        //Dejar paquete en peluqeria o entregar tarde
        [DataType(DataType.MultilineText)]
        [Display(Name = "Comentarios")]
        public string Remarks { get; set; }

        public ICollection<TemporalSale> TemporalSales { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Display(Name = "Cantidad")]
        public float Quantity => TemporalSales == null ? 0 : TemporalSales.Sum(ts => ts.Quantity);

        //Total compra
        [DisplayFormat(DataFormatString = "{0:C2}")]
        [Display(Name = "Valor")]
        public decimal Value => TemporalSales == null ? 0 : TemporalSales.Sum(ts => ts.Value);
    }
}
