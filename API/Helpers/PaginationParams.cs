using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class PaginationParams
    {
        
        private const int maxPageSize = 100;
        public int PageNumber { get; set; } = 1;
        
        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value >= maxPageSize) ? maxPageSize : value; 
        }
    }
}