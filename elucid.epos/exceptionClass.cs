using System;
using System.Collections.Generic;
using System.Text;
//using System.Exception;

namespace epos
{
	class exceptionClass
	{
		//Public Class cSuccessException
		//PickTicketToElucid calls a COM object which generates an error dialog unless
		//an exception is raised. This custom exception class is used to indicate
		//that successful execution has taken place whilst allowing an exception to be
		//generated in all cases when exiting
		//------------------------------------------------------------------------------
		//Date         Developer    Comments
		//25/3/9       RSouthworth  Initial Version
		//------------------------------------------------------------------------------

		//Public Sub New()
		//  MyBase.New()
		//End Sub

		//Public Sub New(ByVal Message As String)
		//  MyBase.New(Message)
		//End Sub

		//Public Sub New(ByVal Message As String, ByVal InnerException As Exception)
		//  MyBase.New(Message, InnerException)
		//End Sub

	}

	//void deleteme()
	//{

	//}

	//void CallCom()
	//{
	//    //Imports the order details from the incoming XML into the Elucid database using
	//    //the trh101/trh102 COM object provided by Sanderson.
	//    //Uses more resource intensive late binding as type library could not be generated.
	//    //------------------------------------------------------------------------------
	//    //Date         Developer    Comments
	//    //24/6/8       RSouthworth  Initial Version
	//    //------------------------------------------------------------------------------

	//    //Dim prms() As Object = Nothing
	//    object prms() = null;

	//    //Dim modifier As New System.Reflection.ParameterModifier(7)
	//    System.Reflection.ParameterModifier(7) new System.Reflection.ParameterModifier modifier;

	//    //Dim typTrh As System.Type = Nothing
	//    System.Type typTrh = null;

	//    //Dim objTrh As Object = Nothing
	//    object objTrh = null;

	//    //Dim iRetStatus As Integer = 0
	//    int iRetStatus = 0;

	//    //Dim tRetMessage1 As String = String.Empty
	//    string tRetMessage1 = String.Empty;

	//    //Dim tRetMessage2 As String = String.Empty
	//    string tRetMessage2 = String.Empty;

	//    //Dim modifierArray() As System.Reflection.ParameterModifier
	//    System.Reflection.ParameterModifier modifierArray();

	//    try
	//    {
	//        //Initialise the parameters (some in some out)
	//        //prms = new object() {INPUT_PARAMETER1, INPUT_PARAMETER2, INPUT_PARAMETER3, XmlIn, XmlOut, iRetStatus, tRetMessage2};

	//        ////Get system type name based on prog ID
	//        //typTrh = Type.GetTypeFromProgID(ProgId);

	//        ////Raise a clear error if the COM object is not configured on the box
	//        //if (typTrh == null)
	//        //{
	//        //    System.ApplicationException("The COM object '" + ProgId + "' cannot be created. Is it registered and configured on this machine?");
	//        //    //throw new ApplicationException("The COM object '" & ProgId & "' cannot be created. Is it registered and configured on this machine?");
	//        //}

	//        ////Use an activator to create object of the type
	//        //objTrh = Activator.CreateInstance(typTrh);

	//        ////Specify the the output parameters
	//        //modifier(4) = true;
	//        //modifier(5) = true;
	//        //modifier(6) = true;

	//        ////The parameter modifier struct must be passed as the single element of an array
	//        //modifierArray = New Reflection.ParameterModifier() {modifier};

	//        ////Execute the method on the com object
	//        //tRetMessage1 = typTrh.InvokeMember(COM_METHOD, Reflection.BindingFlags.InvokeMethod, Nothing, objTrh, prms, modifierArray, Nothing, Nothing).ToString() ;

	//        ////Assign returned values
	//        //if (XmlOut != null)
	//        //{
	//        //    XmlOut = CType(prms(3), String);
	//        //}

	//        ////Raise an error if there were problems
	//        //if CType(prms(5), Integer) <> 0
	//        //{
	//        //    Throw New ApplicationException(String.Format("Failed to use trh COM object '{3}'. The returned message was {0}. The returned status was {1}. PickTicketReference {2}", prms(6), prms(5), PickTicketReference, ProgId));
	//        //}
	//    }
	//    catch (Ex Exception)
	//    {
	//        //Helper.cLogging.Instance.TraceWrite(TraceFilter, "Order import failed for trh COM object. The PickTicketReference is:" & PickTicketReference & " : " & Ex.Message);
	//        //Helper.cLogging.Instance.LogError(Ex);
	//        //Throw
	//    }
	//    finally
	//    {
	//        try
	//        {
	//          //Mop up any resources the COM object is hogging whether an exception was raised or not
	//          //System.Runtime.InteropServices.Marshal.ReleaseComObject(objTrh);
	//        }
	//        catch (ex As Exception)
	//        {
	//          //Do nothing
	//        }
	//    }
	//}
}