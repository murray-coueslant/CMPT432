namespace Illumi_CLI {
    string[, ] Image { get; set; }
    class RuntimeImage {
        public RuntimeImage () {
            Image = new string[32, 8];
        }
    }
}