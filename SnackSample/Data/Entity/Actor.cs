using System;
using System.Collections.Generic;

namespace SnackSample.Data.Entity
{
    public class Actor
    {
        public Actor()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public int ApiId { get; set; }

        public string FullName { get; set; }
        public string Bio { get; set; }

        public virtual ICollection<Movie> Movies { get; set; }
    }
}
