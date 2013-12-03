using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NagaraStage {
    namespace Parameter {
        public class TrackNotExistException : Exception {
            public TrackNotExistException()
                : base() {
            }

            public TrackNotExistException(string message)
                : base(message) {
            }

            public TrackNotExistException(string message, Exception innerException)
                : base(message, innerException) { }
        }
    }
}