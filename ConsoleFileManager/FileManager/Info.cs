namespace ConsoleFileManager.FileManager
{
    /// <summary>
    /// Specifies constants that define the type of the stored information.
    /// </summary>
    public enum InfoType
    {
        FileInfo,
        Message,
        Warning,
        Error
    }
    
    /// <summary>
    /// Represents information to show in information window. The information has specified type
    /// and specified data.
    /// </summary>
    public class Info
    {
        private string data;
        
        /// <summary> Get the type of this information. </summary>
        public InfoType Type { get; }

        /// <summary> Get the data of this information in special form depending on the type of this information. </summary>
        public string Data {
            get {
                return Type switch {
                    InfoType.Warning => $"! {data} !",
                    InfoType.Error => $"!!! {data} !!!",
                    _ => data
                };
            }
        }
        
        /// <summary> Get an empty information (with empty data and of Message type). </summary>
        public static Info Empty => new Info("");

        /// <summary> Get value indicating whether this info has no data. </summary>
        public bool IsEmpty => data.Length == 0;

        
        /// <summary>
        /// Initialize a new instance of information with specified data and of Message type.
        /// </summary>
        /// <param name="data">The information data.</param>
        public Info(string data)
        {
            this.data = data;
            Type = InfoType.Message;
        }


        /// <summary>
        /// Initialize a new instance of information with specified data and type.
        /// </summary>
        /// <param name="data">The information data.</param>
        /// <param name="type">The type of the information.</param>
        public Info(string data, InfoType type)
        {
            this.data = data;
            Type = type;
        }
    }
}