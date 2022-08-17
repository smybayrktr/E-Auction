namespace ESourcing.Products.Settings
{
    public class ProductDatabaseSettings : IProductDatabaseSettings
    {
        public string ConnectionStrings { set; get; }
        public string DatabaseName { set; get; }
        public string CollectionName { set; get; }
    }
}
