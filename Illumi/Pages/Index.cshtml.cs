﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Illumi.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            System.Console.WriteLine("Hello!");
        }
    }
}