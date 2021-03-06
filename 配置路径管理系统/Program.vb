Imports System.IO
Imports System.IO.Path
Imports Microsoft.Win32
Imports System.Security.AccessControl

Module Program
	<Runtime.Versioning.SupportedOSPlatform("windows")>
	Sub Main(args As String())
		Select Case args(0)
			Case "安装"
				Dim MATLAB根目录 = args(1)
				Dim 部署目录 = Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "..\部署")
				File.Copy(Combine(部署目录, "PathManagerRC.m"), Combine(MATLAB根目录, "toolbox\local\PathManagerRC.m"), True)
				Dim matlabrc = New FileStream(Combine(MATLAB根目录, "toolbox\local\matlabrc.m"), FileMode.Open)
				Dim 字符串 = New StreamReader(matlabrc).ReadToEnd
				If Not Text.RegularExpressions.Regex.IsMatch(字符串, "^PathManagerRC;$",Text.RegularExpressions.RegexOptions.Multiline ) Then
					Dim 写出字符串 As New Text.StringBuilder
					If Not {vbLf, vbCrLf}.Contains(字符串.Last) Then
						写出字符串.AppendLine()
					End If
					matlabrc.Seek(0, SeekOrigin.End)
					Call New StreamWriter(matlabrc) With {.AutoFlush = True}.Write(写出字符串.Append("PathManagerRC;").ToString)
					matlabrc.Close()
				End If
				File.Copy(Combine(部署目录, "替换savepath.m"), Combine(MATLAB根目录, "toolbox\matlab\general\savepath.m"), True)
				Dim 文件信息 As New FileInfo(Combine(MATLAB根目录, "toolbox\local\pathdef.m"))
				Dim 访问控制 = 文件信息.GetAccessControl
#If DEBUG Then
				Debugger.Launch()
#End If
				访问控制.SetAccessRule(New FileSystemAccessRule("Users", FileSystemRights.Modify, AccessControlType.Allow))
				文件信息.SetAccessControl(访问控制)
			Case "卸载"
				Dim 注册表键 As RegistryKey
				For Each 键名 In Registry.Users.GetSubKeyNames()
					注册表键 = Registry.Users.OpenSubKey(Combine(键名, "Environment"), True)
					If 注册表键 IsNot Nothing Then
						注册表键.DeleteValue("MATLABPATH", False)
					End If
				Next
				Dim 子路径 = Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MATLAB\共享路径.txt")
				If File.Exists(子路径) Then
					'虽然文件不存在不会出错，但目录不存在是会出错的，所以还是要检查存在性
					File.Delete(子路径)
				End If
				Dim MATLAB根目录 = args(1)
				File.Copy(Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "..\部署\原版savepath.m"), Combine(MATLAB根目录, "toolbox\matlab\general\savepath.m"), True)
				子路径 = Combine(MATLAB根目录, "toolbox\local\matlabrc.m")
				File.WriteAllText(子路径, File.ReadAllText(子路径).Replace(vbCrLf & "PathManagerRC;", "").Replace(vbLf & "PathManagerRC;", ""))
				File.Delete(Combine(MATLAB根目录, "toolbox\local\PathManagerRC.m"))
		End Select
	End Sub
End Module
