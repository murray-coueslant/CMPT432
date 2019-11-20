namespace Illumi_CLI {
    class RuntimeImage {
        string[, ] Image { get; set; }
        public RuntimeImage () {
            Image = new string[32, 8];
        }
    }
}