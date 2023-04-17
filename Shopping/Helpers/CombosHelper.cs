﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping.Data;
using Shopping.Data.Entities;

namespace Shopping.Helpers
{
    public class CombosHelper : ICombosHelper
    {
        //Inyeccion de datacontext (categorias ciudades estados)
        private readonly DataContext _context;
        public CombosHelper(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync()
        {
            //una lista de categorias la convertimos en un selectitem
            List<SelectListItem> list = await _context.Categories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
            })
                .OrderBy(c => c.Text)
                .ToListAsync();
            //En la lista, insertame en la posicion 0 el siguiente texto
            list.Insert(0, new SelectListItem { Text = "[Seleccione una categoría]", Value = "0"});

            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCitiesAsync(int stateId)
        {
            List<SelectListItem> list = await _context.Cities
            //Solo me va a devolver ciudades que pertenecen al pais
                .Where(s => s.State.Id == stateId)
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                })
                .OrderBy(c => c.Text)
                .ToListAsync();
            //En la lista, insertame en la posicion 0 el siguiente texto
            list.Insert(0, new SelectListItem { Text = "[Seleccione una ciudad]", Value = "0" });

            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCountriesAsync()
        {
            List<SelectListItem> list = await _context.Countries.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
            })
                .OrderBy(c => c.Text)
                .ToListAsync();
            //En la lista, insertame en la posicion 0 el siguiente texto
            list.Insert(0, new SelectListItem { Text = "[Seleccione un país]", Value = "0" });

            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboStatesAsync(int countryId)
        {
            List<SelectListItem> list = await _context.States
                //Solo me va a devolver estados que pertenecen al pais
                .Where(s => s.Country.Id == countryId)
                .Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
            })
                .OrderBy(c => c.Text)
                .ToListAsync();
            //En la lista, insertame en la posicion 0 el siguiente texto
            list.Insert(0, new SelectListItem { Text = "[Seleccione un Departamento/Estado]", Value = "0" });

            return list;
        }
    }
}

//esta inyección agregarlo al program