using Nearest.Storage;

namespace Nearest.Models
{
    class FavoriteQuery : Storable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Query { get; set; }
    }
}
