using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Brainboxes.IO.Tests
{
    /// <summary>
    /// Encapsulates, expected vs actual output for a particular command
    /// </summary>
    public class CommandResponse
    {
        public CommandResponse(string query, string expected, string comment)
        {
            this.query = query;
            this.expected = expected;
            this.comment = comment;
        }
        public string query { get; set; }
        public string expected { get; set; }
        public string actual { get; set; }
        public string comment { get; set; }
    }

    /// <summary>
    /// provides convenience method to add CommandResponses to List
    /// and RunTest on all commands in list with a given device
    /// </summary>
    public class CommandResponseList : List<CommandResponse>
    {
        public void Add(string query, string response, string comment)
        {
            Add(new CommandResponse(query, response, comment));
        }


        public void RunTest(IEDDevice device)
        {
            //loop through every command first
            foreach (CommandResponse qr in this)
            {
                //sendCommand uses debug console so to keep them correlating do the same here
                System.Diagnostics.Debug.WriteLine("\n======SendCommand====\nComment: " + qr.comment);
                qr.actual = device.SendCommand(qr.query);
                if (qr.expected != qr.actual)
                {
                    System.Diagnostics.Debug.WriteLine(" * * * unexpected response ... expected: " + qr.expected);
                }
            }
            //then assert responses
            foreach (CommandResponse qr in this)
            {
                Assert.AreEqual(qr.expected, qr.actual, "For query " + qr.query + " comment: " + qr.comment);
            }
        }
    }
}
