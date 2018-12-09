using System.Collections.Generic;
using SixLabors.ImageSharp;

namespace MemeGenerator
{
    public class MemeTemplateBuilder
    {
        private readonly string _imagePath;
        private readonly List<InputField> _inputFields;

        public MemeTemplateBuilder(string imagePath)
        {
            _imagePath = imagePath;
            _inputFields = new List<InputField>();
        }

        public MemeTemplateBuilder WithInputField(InputField inputField)
        {
            _inputFields.Add(inputField);
            return this;
        }
        
        public MemeTemplate Build()
        {
            var img = Image.Load(_imagePath);
            return new MemeTemplate(img, _inputFields);
        }
    }
}
