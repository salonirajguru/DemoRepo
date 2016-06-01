using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using Crest.Impersonation;
using Crest;
using System.Configuration;

namespace ExportEpubToFolder
{
    public class ExportEpub
    {
        static public void sendEmail(string exceptionmessage, string jobid)
        {
            try
            {
                string filename = "";
                DatabaseConnection objD = new DatabaseConnection();
                string fromemail = ConfigurationManager.AppSettings["fromemail"];
                string exceptionsubject = ConfigurationManager.AppSettings["exceptionsubject"];
                string exceptionccemail = ConfigurationManager.AppSettings["exceptionccemail"];
                string exceptiontoemail = ConfigurationManager.AppSettings["exceptiontoemail"];
                string exceptionbccemail = ConfigurationManager.AppSettings["exceptionbccemail"];
                exceptionsubject = exceptionsubject + "  -  " + filename;
                string emailbody = "<html><body>Dear Sir/Madam,<br/><br/>Exception is coming in epub Export service  <br/><br/><br/><br/>" + exceptionmessage.Replace("'", "") + jobid + "<br/><br/><br/><br/>Regards,<br/>ePub Creation<br/>CREST<br/><br/><br/><b><i>This is a system generated notification, kindly revert to individuals in case of any clarification</i></b></body></html>";
                string query = "insert into dbproduction.tblemail (fromadd,toadd,mailsubject,cc,bcc,body,status) values('" + fromemail + "','" + exceptiontoemail + "','" + exceptionsubject + "','" + exceptionccemail + "','" + exceptionbccemail + "','" + emailbody + "','notsend')";
                objD.Insert(query);
            }
            catch (Exception)
            {
                //throw;
            }
        }
        static void Main(string[] args)
        {
            string epubPackageCreation = "false";
            string logFile = string.Empty;
            DatabaseConnection dbConnect = new DatabaseConnection();
            DataTable autodispatch = new DataTable();
            DataTable jobsheetidDt = new DataTable();
            string _jobsheetid = "";
            string autoprodid = "";
            string _epubfilename = "";
            string hardrivepath = "";
            string epublogfilename = "";
            string _jobid = "";
            string _epubstatus = "";
            string flag = "true";
            string epubpackagehardrivepath1 = String.Empty;
            string _hardDrivePath = "";
            string userName = ConfigurationManager.AppSettings["userName"];
            string password = ConfigurationManager.AppSettings["password"];
            string domain = ConfigurationManager.AppSettings["domain"];
            Impersonate objImp = new Impersonate(userName, domain, password);
            try
            {
                string querystatus = "select auto.EpubStatus,auto.jobid,jsi.HardDrivepath,jsi.JobsheetID,auto.prodid from dbproduction.tblautodispatch600and650_epub auto join newdbcrest.jobsheetinfo jsi on (auto.jobid=jsi.jobid)  where auto.EpubStatus='InProcess' and IsEpub='1'";
                autodispatch = dbConnect.getData(querystatus);
                if (objImp.StartImpersonation(userName, domain, password))
                {
                    for (int i = 0; i <= autodispatch.Rows.Count - 1; i++)
                    {
                        flag = "true";
                        epubPackageCreation = "false";
                        _epubstatus = Convert.ToString(autodispatch.Rows[i]["EpubStatus"]);
                        _jobid = Convert.ToString(autodispatch.Rows[i]["Jobid"]);
                        hardrivepath = Convert.ToString(autodispatch.Rows[i]["HardDrivePath"]);
                        _jobsheetid = Convert.ToString(autodispatch.Rows[i]["JobSheetID"]);
                        autoprodid = Convert.ToString(autodispatch.Rows[i]["prodid"]);
                        _epubfilename = Convert.ToString(_jobsheetid + ".epub ");
                        epublogfilename = Convert.ToString(_jobsheetid + "-epub.log ");
                        if (_epubstatus.Equals("InProcess"))
                        {

                            _hardDrivePath = (ConfigurationManager.AppSettings["ePubInputFolderOutputPath"]);
                            //  _hardDrivePath = (ConfigurationManager.AppSettings["epubout"]);
                            epubpackagehardrivepath1 = Path.GetDirectoryName(Path.GetDirectoryName(hardrivepath)) + "\\epub";
                            epubpackagehardrivepath1 = epubpackagehardrivepath1.Replace("R:", @"\\192.168.84.53\pts");
                            //    if (objImp.StartImpersonation(userName, domain, password))
                            //  {
                            Directory.CreateDirectory(epubpackagehardrivepath1);
                            if (Directory.Exists(_hardDrivePath))
                            {
                                string[] zipFilePath = Directory.GetFiles(_hardDrivePath, "*.epub");


                                if (zipFilePath.Count() > 0)
                                {
                                    string epubfilepath = Convert.ToString(_hardDrivePath + "\\" + _epubfilename);
                                    string logfilepath = Convert.ToString(_hardDrivePath + "\\" + epublogfilename);

                                    string[] epubfilepackage = Directory.GetFiles(_hardDrivePath, "*.epub");
                                    string[] logfileinpackage = Directory.GetFiles(_hardDrivePath, "*.log");
                                    //  string epub = Path.GetDirectoryName(_hardDrivePath);
                                    //    string epubpackagehardrivepath1=Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(hardrivepath))) + "\\EpubPackage";   requ
                                    if (File.Exists(epubfilepath))
                                    {
                                        if (Directory.Exists(epubpackagehardrivepath1))
                                        {
                                            foreach (string match in epubfilepackage)
                                            {
                                                string res = Path.GetFileName(match);
                                                if (res.Trim().Equals(_epubfilename.Trim()))
                                                {
                                                    flag = "true";
                                                    epubPackageCreation = "true";
                                                    break;
                                                }
                                                else
                                                {
                                                    continue;
                                                }

                                            }
                                            if (flag == "true")
                                            {
                                                String getfile = Path.GetFullPath(epubpackagehardrivepath1 + "\\" + _epubfilename);
                                                if (File.Exists(getfile))
                                                {
                                                    File.Copy(epubfilepath, epubpackagehardrivepath1 + "\\" + Path.GetFileName(epubfilepath));
                                                    string updatequery = "update dbproduction.tblautodispatch600and650_epub set EpubStatus='Done' where jobid='" + _jobid + "'";
                                                    string jobrevisionquery = "update dbproduction.tbljobrevisionmaster set status='open' where status='Epub In Process' and prodid='" + autoprodid + "'";
                                                    dbConnect.Update(updatequery);
                                                    dbConnect.Update(jobrevisionquery);
                                                    epubPackageCreation = "true";
                                                }
                                                else
                                                {
                                                    File.Copy(epubfilepath, epubpackagehardrivepath1 + "\\" + Path.GetFileName(epubfilepath));
                                                    string updatequery = "update dbproduction.tblautodispatch600and650_epub set EpubStatus='Done' where jobid='" + _jobid + "'";
                                                    string jobrevisionquery = "update dbproduction.tbljobrevisionmaster set status='open' where status='Epub In Process' and prodid='" + autoprodid + "'";
                                                    dbConnect.Update(updatequery);
                                                    dbConnect.Update(jobrevisionquery);
                                                    epubPackageCreation = "true";
                                                }
                                            }
                                        }
                                        else
                                        {

                                        }
                                    }
                                    else
                                    {
                                        //   logFile += Convert.ToString(DateTime.Now) + " Error:File is not present" + epubfilepath;
                                    }

                                    flag = "false";
                                    if (File.Exists(logfilepath))
                                    {
                                        if (Directory.Exists(epubpackagehardrivepath1))
                                        {
                                            foreach (string matchlog in logfileinpackage)
                                            {
                                                string res = Path.GetFileName(matchlog);
                                                if (res.Trim().Equals(epublogfilename.Trim()))
                                                {
                                                    flag = "true";
                                                    epubPackageCreation = "true";
                                                    break;
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                            if (flag == "true")
                                            {
                                                String getfile = Path.GetFullPath(epubpackagehardrivepath1 + "\\" + epublogfilename);
                                                if (File.Exists(getfile))
                                                {
                                                    File.Copy(logfilepath, epubpackagehardrivepath1 + "\\" + Path.GetFileName(logfilepath));
                                                    string updatequery = "update dbproduction.tblautodispatch600and650_epub set EpubStatus='Done' where jobid='" + _jobid + "'";
                                                    string jobrevisionquery = "update dbproduction.tbljobrevisionmaster set status='open' where status='Epub In Process' and prodid='" + autoprodid + "'";
                                                    dbConnect.Update(updatequery);
                                                    dbConnect.Update(jobrevisionquery);
                                                    epubPackageCreation = "true";
                                                }
                                                else
                                                {
                                                    File.Copy(logfilepath, epubpackagehardrivepath1 + "\\" + Path.GetFileName(logfilepath));
                                                    string updatequery = "update dbproduction.tblautodispatch600and650_epub set EpubStatus='Done' where jobid='" + _jobid + "'";
                                                    string jobrevisionquery = "update dbproduction.tbljobrevisionmaster set status='open' where status='Epub In Process' and prodid='" + autoprodid + "'";
                                                    dbConnect.Update(updatequery);
                                                    dbConnect.Update(jobrevisionquery);
                                                    epubPackageCreation = "true";

                                                }
                                            }
                                        }
                                        else
                                        {
                                            // logFile += Convert.ToString(DateTime.Now) + " Error:Folder is not present" + epubpackagehardrivepath1;
                                        }
                                    }

                                }
                            }
                            else
                            {
                                // logFile += Convert.ToString(DateTime.Now) + " Error:Folder is not present" + _hardDrivePath;
                            }
                            //   }
                            //else
                            //{
                            //    // logFile += Convert.ToString(DateTime.Now) + " Impersonation is not working";

                            //}

                        }
                        if (epubPackageCreation == "false")
                        {
                            sendEmail("Error for jobid = ", _jobid);

                        }
                    }
                    //if (logFile.Contains("Error"))
                    //{
                    //    using (StreamWriter sw = new StreamWriter(epubpackagehardrivepath1 + "\\Logfile.txt"))
                    //    {
                    //        //      sw.WriteLine(logFile);
                    //    }
                    //}
                }
            }

            catch (Exception ex)
            {
                //throw
            }
            finally
            {
                if (objImp.ImpersonationActive)
                    objImp.EndImpersonation();
            }

        }
    }
}
