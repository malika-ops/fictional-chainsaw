using System.ComponentModel;

namespace wfc.referential.Application
{
    public record FilterRequest
    {
        private int? _pageNumber;
        private int? _pageSize;
        private bool? _isEnabled;

        /// <summary>
        /// The page number to retrieve (default is 1).
        /// </summary>
        /// <example>1</example>
        [DefaultValue(1)]
        public int? PageNumber
        {
            get => _pageNumber ?? 1;
            init => _pageNumber = value;
        }

        /// <summary>
        /// The number of records to retrieve per page (default is 10).
        /// </summary>
        /// <example>10</example>
        [DefaultValue(10)]
        public int? PageSize
        {
            get => _pageSize ?? 10;
            init => _pageSize = value;
        } 
        
        /// <summary>
        /// The Filter to use if you want to include desabled entities or not
        /// </summary>
        /// <example>10</example>
        [DefaultValue(true)]
        public bool? IsEnabled
        {
            get => _isEnabled ?? true;
            init => _isEnabled = value;
        }
    }
}
