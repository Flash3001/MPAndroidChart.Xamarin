using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MikePhil.Charting.Buffer
{
    public partial class CandleShadowBuffer
    {
        static IntPtr id_feed_Ljava_util_List_;
        // Metadata.xml XPath method reference: path="/api/package[@name='com.github.mikephil.charting.buffer']/class[@name='CandleShadowBuffer']/method[@name='feed' and count(parameter)=1 and parameter[1][@type='java.util.List&lt;com.github.mikephil.charting.data.CandleEntry&gt;']]"
        [Register("feed", "(Ljava/util/List;)V", "GetFeed_Ljava_util_List_Handler")]
        public unsafe void Feed(global::System.Collections.Generic.IList<MikePhil.Charting.Data.CandleEntry> p0)
        {
            if (id_feed_Ljava_util_List_ == IntPtr.Zero)
                id_feed_Ljava_util_List_ = JNIEnv.GetMethodID(class_ref, "feed", "(Ljava/util/List;)V");
            IntPtr native_p0 = global::Android.Runtime.JavaList<MikePhil.Charting.Data.CandleEntry>.ToLocalJniHandle(p0);
            try
            {
                JValue* __args = stackalloc JValue[1];
                __args[0] = new JValue(native_p0);

                if (GetType() == ThresholdType)
                    JNIEnv.CallVoidMethod(Handle, id_feed_Ljava_util_List_, __args);
                else
                    JNIEnv.CallNonvirtualVoidMethod(Handle, ThresholdClass, JNIEnv.GetMethodID(ThresholdClass, "feed", "(Ljava/util/List;)V"), __args);
            }
            finally
            {
                JNIEnv.DeleteLocalRef(native_p0);
            }
        }

        public override void Feed(IList p0)
        {
            Feed(p0 as IList<MikePhil.Charting.Data.CandleEntry>);
        }
    }
}