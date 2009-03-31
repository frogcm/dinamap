using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Text;
using System.Data.Odbc;

namespace DinamapN
{
    public partial class frmMeasurement : Form
    {
        //Variable Global
        int numMeasurements;
        int numMeasurementsSuccessful;
        int numMeasurementsFailed;
        int numCommentMarker;
        private string patientID;
        private string studyID;
        private XmlDocument lastMeasurement = new XmlDocument();
        private OdbcConnection MyConnection;

        public frmMeasurement()
        {
            InitializeComponent();
            
            //FOR DEBUGGING ONLY!
            
            
            // Store Patient & Study ID's for later use
            patientID = "123";
            studyID = "4125";

            // Show ID's on form
            lblPatientID.Text = patientID;
            lblStudyID.Text = studyID;

            Directory.CreateDirectory("C:\\" + studyID + "_" + patientID);
            Directory.CreateDirectory("C:\\" + studyID + "_" + patientID + "\\raw_xml");
            Directory.CreateDirectory("C:\\" + studyID + "_" + patientID + "\\queued_sql");
            

        }

        public frmMeasurement(string patient, string study)
        {
            InitializeComponent();
            
            // Store Patient & Study ID's for later use
            patientID = patient;
            studyID = study;

            // Show ID's on form
            lblPatientID.Text = patient;
            lblStudyID.Text = study;
            
            // Load reference XML (necessary for first comparison)
            try
            {
                lastMeasurement.Load("C:\\dinamap.xml");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load reference XML file: " + ex.ToString());
            }

        }

        // If someone clicks "Start"...
        private void cmdStart_Click(object sender, EventArgs e)
        {
            // Check that Dinamap is connected if desired.  Break if not.
            if (dinamapConnectedCheckBox.Checked && !Tool.Dina_CheckReadiness())
            {
                MessageBox.Show("Dinamap machine not ready!  Please check power, connection, or ensure that USB-serial adapter driver is installed.");
                return;
            }
	        int interval = 10000;  // Set time interval for measurements (10 seconds)
	        measurementTimer.Enabled = true; // Enable timer
            measurementTimer.Interval = interval; // Assign interval to timer
            measurementTimer.Start(); // Begin timer
            cmdStart.Enabled = false; // Disable "Start" Icon
            cmdStop.Enabled = true; // Enable "Stop" Icon
            dinamapConnectedCheckBox.Enabled = false;
        }

        private void measurementTimer_Tick(object sender, EventArgs e)
        {
            XmlDocument currentMeasurement = new XmlDocument();

            // If dinamap not connected, use different pull function for debugging
            if (dinamapConnectedCheckBox.Checked)
                currentMeasurement = Tool.Dina_GetStateOn();
            else
                currentMeasurement = Tool.Dina_GetStateOff();

            // If pulled measurement is not the same as the last one, handle it
            if (currentMeasurement.InnerText != lastMeasurement.InnerText)
            {
                Hashtable h = this.handleResponse();
            }
        }

        // If someone clicks "Stop"...
        private void cmdStop_Click(object sender, EventArgs e)
        {
            measurementTimer.Stop(); // cease taking measurements
            cmdStop.Enabled = false; // Disable "Stop" icon
            cmdStart.Enabled = true; // Enable "Start" icon
            dinamapConnectedCheckBox.Enabled = true;
            uploadAllComments(); // Upload comments
        }

        private Hashtable handleResponse()
        {
            Hashtable h = new Hashtable();
            if (dinamapConnectedCheckBox.Checked)
                lastMeasurement = Tool.Dina_GetStateOn();
            else
                lastMeasurement = Tool.Dina_GetStateOff();

            this.saveLocalXML(lastMeasurement);
            h = responseToHash(lastMeasurement);
            this.saveMySQL(h);
            //this.saveAccess(h);
            return h;
        }

        // Saves XML from measurement locally
        private void saveLocalXML(XmlDocument doc)
        {
            doc.PreserveWhitespace = false;
            try
            {
                doc.Save("C:\\" + studyID + "_" + patientID + "\\raw_xml\\" + numMeasurements + ".xml");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error saving XML locally: " + ex.ToString());
            }
        }

        private void saveMySQL(Hashtable h)
        {
            string query = buildQueryString(h, false);

            try
            {
                OdbcConnection MyConnection = new OdbcConnection("DSN=dinamapMySQL2");
                MyConnection.Open();
                OdbcCommand DbCommand = MyConnection.CreateCommand();
                DbCommand.CommandText = query;
                DbCommand.ExecuteNonQuery();
                writeToGrid(true, h);
            }
            catch (Exception)
            {
                saveLocalSQL(query);
                writeToGrid(false, h);
            }
        }

        public void writeToGrid(bool success, Hashtable h)
        {
            // Show measurement with "success" icon and field
            if (success)
            {
                this.mGrid.Rows.Add(DinamapN.Properties.Resources.successful, ((DateTime)h["Systolic_blood_pressure_Time_stamp"]),
                    h["Systolic_blood_pressure_Value"],
                    h["Diastolic_blood_pressure_Value"], h["Pulse_Value"], "",true);
                numMeasurementsSuccessful++;
            }
            // Show measurement with "failed" icon and field
            else
            {
                this.mGrid.Rows.Add(DinamapN.Properties.Resources.error, ((DateTime)h["Systolic_blood_pressure_Time_stamp"]),
                    h["Systolic_blood_pressure_Value"],
                    h["Diastolic_blood_pressure_Value"], h["Pulse_Value"], "",false);
                numMeasurementsFailed++;
            }
            // Update measurement stats and display
            numMeasurements++;
            lblNum.Text = numMeasurements.ToString();
            toolStripStatusLabelNumSuccessful.Text = numMeasurementsSuccessful.ToString();                    
            toolStripStatusLabelNumFailed.Text = numMeasurementsFailed.ToString();
        }


/*
        private void saveAccess(Hashtable h)
       {
            string query = buildQueryString(h,true);
            MessageBox.Show(query);
            try
            {
                OdbcConnection MyConnection = new OdbcConnection("DSN=dinamap");
                MyConnection.Open();
                OdbcCommand DbCommand = MyConnection.CreateCommand();
                DbCommand.CommandText = query;
                DbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
*/

        // Constructs query from hashtable built from a successful measurement
        private string buildQueryString(Hashtable h, Boolean access)
        {
            StringBuilder queryBuilder = new StringBuilder();

            try
            {
                // "Time" is reserved keyword for MsAccess, use "MeasurementTime instead
                //if (access)
                //    queryBuilder.Append("INSERT INTO MeasurementsData (Study_ID, MeasurementTime, SP, DP, MAP, Pulse, Comments) VALUES");
                //else
                    queryBuilder.Append("INSERT INTO MeasurementsData (Study_ID, Time, SP, DP, MAP, Pulse, Comments) VALUES");
                queryBuilder.Append("(");
                queryBuilder.Append("'");
                queryBuilder.Append(studyID);
                queryBuilder.Append("','");
                // Use different date/time convention for MS access
                //if (access)
                //    queryBuilder.Append(((DateTime)h["Systolic_blood_pressure_Time_stamp"]).ToString("MM/dd/yyyy HH:mm:ss"));
                //else
                    queryBuilder.Append(((DateTime)h["Systolic_blood_pressure_Time_stamp"]).ToString("yyyy:MM:dd HH:mm:ss"));
                queryBuilder.Append("','");
                queryBuilder.Append(h["Systolic_blood_pressure_Value"]);
                queryBuilder.Append("','");
                queryBuilder.Append(h["Diastolic_blood_pressure_Value"]);
                queryBuilder.Append("','");
                queryBuilder.Append(h["Mean_arterial_pressure_Value"]);
                queryBuilder.Append("','");
                queryBuilder.Append(h["Pulse_Value"]);
                queryBuilder.Append("','");             
                queryBuilder.Append("'");
                queryBuilder.Append(");");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error building SQL query string: " + ex.ToString());
            }

            return queryBuilder.ToString();
        }

        private Hashtable responseToHash(XmlDocument doc)
        {
            Hashtable h = new Hashtable();

            XmlNodeList doc_results = doc.GetElementsByTagName("Result");

            foreach (XmlNode pnode in doc_results)
            {
                foreach (XmlNode cnode in pnode.ChildNodes)
                {
                    if(cnode.Name == "Units")
                    {
                        h.Add(pnode.Attributes["name"].InnerText + "_" + cnode.Name, cnode.Attributes["name"].InnerText);
                    }
                    else if(cnode.Name == "Time_stamp")
                    {
                        DateTime d = new DateTime(
                            Convert.ToInt32(cnode.Attributes["year"].InnerText),
                            Convert.ToInt32(cnode.Attributes["month"].InnerText),
                            Convert.ToInt32(cnode.Attributes["day"].InnerText),
                            Convert.ToInt32(cnode.Attributes["hour"].InnerText),
                            Convert.ToInt32(cnode.Attributes["minute"].InnerText),
                            Convert.ToInt32(cnode.Attributes["second"].InnerText)); 
                        
                        h.Add(pnode.Attributes["name"].InnerText + "_" + cnode.Name, d);
                    }
                    else
                        h.Add(pnode.Attributes["name"].InnerText + "_" + cnode.Name, cnode.InnerText);
                } 
            }

            return h;
        }

        private void frmMeasurement_Load(object sender, EventArgs e)
        {
            cmdStart.Enabled = true;
            cmdStop.Enabled = false;
            numMeasurements = 0;
            numCommentMarker = 0;

            // Load database connection
            try
            {
                MyConnection = new OdbcConnection("DSN=dinamapMySQL2");
            }
            catch
            {
                MessageBox.Show("DSN does not exist or contains errors.  See administrator.  Measurements will be saved locally.");
            }
        }

        private void sysTime_Tick(object sender, EventArgs e)
        {
            string szHour;
            szHour = DateTime.Now.ToString("h:mm:ss");
            lblTime.Text = szHour;
        }

        private void frmMeasurement_Activated(object sender, System.EventArgs e)
        {
            sysTimer.Start();
        }

        // Change comment cell color after user enters something
        private void mGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (mGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue.Equals(""))
            {
                mGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.White;
            }
            else
            {
                mGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.Yellow;
            }
            mGrid.ClearSelection();
        }
        
        // Attempt to upload comments to database
        private void uploadAllComments()
        {
            DataGridViewRow inputRow;
            string valueUploadStatus;
            string insertStatement;
            string commentText;

            // Go through all rows, start after the last one handled
            for (int i = numCommentMarker; i < numMeasurements; i++)
            {
                numCommentMarker++;  // Show this row was handled
                inputRow = mGrid.Rows[i]; // pull row
                valueUploadStatus = inputRow.Cells[6].FormattedValue.ToString(); // Find if measurement was uploaded
                commentText = inputRow.Cells[5].FormattedValue.ToString(); // Pull comment
                insertStatement = buildCommentSQL(inputRow); // Build SQL string

                // Only attempt upload if comment exists and measurement was
                // successfully uploaded
                if (valueUploadStatus.Equals("True") && !commentText.Equals(""))
                {
                    // Upload value 
                    try
                    {
                        MyConnection.Open();
                        OdbcCommand DbCommand = MyConnection.CreateCommand();
                        DbCommand.CommandText = insertStatement;
                        DbCommand.ExecuteNonQuery();
                        MyConnection.Close();
                        mGrid.Rows[i].Cells[5].Style.BackColor = Color.Green;
                    }
                    // Save locally if unsuccessful
                    catch(Exception ex)
                    {
                        saveLocalSQL(insertStatement);
                        MessageBox.Show(ex.ToString());
                        mGrid.Rows[i].Cells[5].Style.BackColor = Color.Red;
                    }
                }
                // Save locally if measurement failed.
                else if (valueUploadStatus.Equals("False"))
                {
                    MessageBox.Show("Entry " + i.ToString() + " value record failed to upload previously.  Comment upload command stored locally.");
                    saveLocalSQL(insertStatement);
                }
            }
        }

        // Constructs SQL update statement string  to commit comments
        // for a given row from the grid viewer
        private string buildCommentSQL(DataGridViewRow inputRow)
        {
            string commentText = inputRow.Cells[5].FormattedValue.ToString(); // Grab comment from row
            string commentTime = ((DateTime)inputRow.Cells[1].Value).ToString("yyyy:MM:dd HH:mm:ss"); // Grab date from row, convert for SQL
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append("UPDATE MeasurementsData SET Comments = '");
            queryBuilder.Append(commentText);
            queryBuilder.Append("' WHERE ((Study_ID = '");
            queryBuilder.Append(studyID);
            queryBuilder.Append("') AND (Time = '");
            queryBuilder.Append(commentTime);
            queryBuilder.Append("'));");
            return queryBuilder.ToString();
        }

        // Saves SQL statements to local directory for uploading later
        private void saveLocalSQL(string statement)
        {
            try
            {
                StreamWriter output = new StreamWriter("C:\\" + studyID + "_" + patientID + "\\queued_sql\\" + "queued_sql.sql", true);
                output.WriteLine(statement);
                output.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving SQL locally! " + ex.ToString());
            }
        }

        private void mGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            mGrid.BeginEdit(true);
        }

        private void headPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void bodyPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}