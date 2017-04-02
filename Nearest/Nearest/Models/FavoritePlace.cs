using Nearest.GoogleApi.Models;
using Nearest.Storage;

namespace Nearest.Models
{
    class FavoritePlace : Storable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Place Place { get; set; }
        public string Description
        {
            get
            {
                return Place.name + "(" + Place.geometry.location.lat + ", " + Place.geometry.location.lng + ")";
            }
            private set { }
        }
    }
}
