using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;

namespace MemeGenerator
{
    public class MemeTemplateBuilder
    {
        private readonly string _imagePath;
        private readonly List<InputField> _inputFields;
        private string _name;
        private string _description;

        public MemeTemplateBuilder(string imagePath)
        {
            _imagePath = imagePath;
            _inputFields = new List<InputField>();
            _name = "";
            _description = "";
        }

        public MemeTemplateBuilder WithName(string name)
        {
            _name = name ?? throw new ArgumentNullException($"{nameof(name)} cannot be null.");
            return this;
        }

        public MemeTemplateBuilder WithDescription(string description = null)
        {
            _description = description ?? "";
            return this;
        }

        public MemeTemplateBuilder WithInputField(InputField inputField)
        {
            if (inputField == null)
            {
                throw new ArgumentNullException($"{nameof(inputField)} cannot be null.");
            }
            _inputFields.Add(inputField);
            return this;
        }
        
        public MemeTemplate Build()
        {
            return new MemeTemplate(_name, _description, _imagePath, _inputFields);
        }
    }
}
