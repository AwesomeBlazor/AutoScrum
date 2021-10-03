using AutoScrum.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace AutoScrum.Components
{
    public partial class UserBlocking
    {
        [Parameter]
        public List<User> Users { get; set; }
    }
}
