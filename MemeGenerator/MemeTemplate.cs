using System;

namespace MemeGenerator
{
    public class MemeTemplate
    {
        private readonly string _imagePath;
        private readonly InputField[] _boundingBoxes;

        public MemeTemplate(string imagePath, params InputField[] boundingBoxes)
        {
            _imagePath = imagePath;
            _boundingBoxes = boundingBoxes;
        }
    }
}
