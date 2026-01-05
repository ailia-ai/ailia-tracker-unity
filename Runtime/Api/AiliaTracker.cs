/**
* \~japanese
* @file
* @brief AILIA Unity Plugin Tracker API
* @author AXELL Corporation
* @date  Jan 6, 2026
* 
* \~english
* @file
* @brief AILIA Unity Plugin Tracker API
* @author AXELL Corporation
* @date  Jan 6, 2026
*/

using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Runtime.InteropServices;

using ailia;
namespace ailiaTracker{

public class AiliaTracker
{
    /* Native Binary 定義 */

#if (UNITY_IPHONE && !UNITY_EDITOR) || (UNITY_WEBGL && !UNITY_EDITOR)
        public const String LIBRARY_NAME="__Internal";
#else
#if (UNITY_ANDROID && !UNITY_EDITOR) || (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX)
            public const String LIBRARY_NAME="ailia_tracker";
#else
    public const String LIBRARY_NAME = "ailia_tracker";
#endif
#endif

    /****************************************************************
    * アルゴリズム定義
    **/

    /**
    * \~japanese
    * @def AILIA_TRACKER_ALGORITHM_BYTE_TRACK
    * @brief ByteTrack
    *
    * \~english
    * @def AILIA_TRACKER_ALGORITHM_BYTE_TRACK
    * @brief ByteTrack
    */
    public const int AILIA_TRACKER_ALGORITHM_BYTE_TRACK = (0);

    /****************************************************************
    * 物体情報
    **/

    /**
    * \~japanese
    * @def AILIA_TRACKER_OBJECT_VERSION
    * @brief Object version
    *
    * \~english
    * @def AILIA_TRACKER_OBJECT_VERSION
    * @brief Object version
    */
    public const int AILIA_TRACKER_OBJECT_VERSION = (1);

    [StructLayout(LayoutKind.Sequential)]
    public class AILIATrackerObject
    {
        /**
         * \~japanese
         * オブジェクトのトラッキングID
         *
         * \~english
         * Object tracking id
         */
        public uint id;
        /**
         * \~japanese
         * オブジェクトカテゴリ番号(0～category_count-1)
         *
         * \~english
         * Object category number (0 to category_count-1)
         */
        public uint category;
        /**
         * \~japanese
         * 推定確率(0～1)
         *
         * \~english
         * Estimated probability (0 to 1)
         */
        public float prob;
        /**
         * \~japanese
         * 左上X位置(1で画像幅)
         *
         * \~english
         * X position at the top left (1 for the image width)
         */
        public float x;
        /**
         * \~japanese
         * 左上Y位置(1で画像高さ)
         *
         * \~english
         * Y position at the top left (1 for the image height)
         */
        public float y;
        /**
         * \~japanese
         * 幅(1で画像横幅、負数は取らない)
         *
         * \~english
         * Width (1 for the width of the image, negative numbers not allowed)
         */
        public float w;
        /**
         * \~japanese
         * 高さ(1で画像高さ、負数は取らない)
         *
         * \~english
         * Height (1 for the height of the image, negative numbers not allowed)
         */
        public float h;
    }

    /****************************************************************
    * 設定情報
    **/

   /**
    * \~japanese
    * @def AILIA_TRACKER_SETTINGS_VERSION
    * @brief Settings version
    *
    * \~english
    * @def AILIA_TRACKER_SETTINGS_VERSION
    * @brief Settings version
    */
    public const int AILIA_TRACKER_SETTINGS_VERSION = (1);

    [StructLayout(LayoutKind.Sequential)]
    public class AILIATrackerSettings
    {
        /**
        * \~japanese
        * 検出結果を有効とみなすスコア（信頼度）の下限値です。  
        * この値より小さいスコアの検出は無視されます。  
        * デフォルト値：0.1
        *
        * \~english
        * Minimum confidence score to accept a detection result.  
        * Detections with scores below this value are ignored.  
        * Default: 0.1
        */
        public float score_threshold;
        /**
        * \~japanese
        * 重複する検出を除去するための NMS（非最大抑制）しきい値です。  
        * Intersection over Union (IoU) がこの値を超える検出結果は重複として統合されます。  
        * デフォルト値：0.7
        *
        * \~english
        * Non‑Maximum Suppression (NMS) threshold used to remove duplicate detections.  
        * Detections whose Intersection over Union (IoU) exceeds this value are merged.  
        * Default: 0.7
        */
        public float nms_threshold;
        /**
        * \~japanese
        * トラッキング対象として更新に使用する検出スコアのしきい値です。  
        * 通常はこの値以上のスコアを持つ検出のみでトラックを更新しますが、  
        * ByteTrack の手法では、より低いスコア（`score_threshold` 以上～この値未満）の検出であっても  
        * IoU によるマッチングが成立した場合は補助的にトラック更新に利用されることがあります。  
        * 値を高くすると厳密な追跡となり、低くすると一時的に信頼度が下がった物体も追跡しやすくなります。  
        * デフォルト値：0.5
        *
        * \~english
        * Confidence threshold for updating active tracks.  
        * Normally, only detections with scores above this value are used to update existing tracks.  
        * However, in the ByteTrack approach, detections with lower scores (between `score_threshold` and this value)  
        * may also be used for updates if they achieve sufficient IoU matching with existing tracks.  
        * A higher value makes the tracking stricter, while a lower value allows objects with temporarily low confidence to be maintained.  
        * Default: 0.5
        */
        public float track_threshold;
        /**
        * \~japanese
        * 見失ったトラック情報を保持しておく最大フレーム数です。  
        * この間に同一物体を再検出できた場合、トラッキングを再開します。  
        * デフォルト値：30
        *
        * \~english
        * Maximum number of frames to keep lost tracks in memory.  
        * If the same object is detected again within this period, tracking is resumed.  
        * Default: 30
        */
        public int track_buffer;
        /**
        * \~japanese
        * フレーム間での物体対応付け（マッチング）を行う際の IoU しきい値です。  
        * IoU がこの値以上の場合、同一物体としてマッチングされます。  
        * 値を大きくするとマッチングが厳密になります。  
        * デフォルト値：0.8
        *
        * \~english
        * IoU threshold used for matching objects between frames.  
        * Detections with an IoU greater than or equal to this value are treated as the same object.  
        * Higher values make the matching stricter.  
        * Default: 0.8
        */
        public float match_threshold;
    }

    /****************************************************************
    * フラグ定義
    **/

    /**
    * \~japanese
    * @def AILIA_TRACKER_FLAG_NONE
    * @brief フラグを設定しません
    *
    * \~english
    * @def AILIA_TRACKER_FLAG_NONE
    * @brief Default flag
    */
    public const int AILIA_TRACKER_FLAG_NONE = (0);

    /**
    * \~japanese
    * @def AILIA_TRACKER_FLAG_ALLOW_WIDE_ASPECT_RATIO
    * @brief 通常、人物トラッキングでは縦横比（幅／高さ）が 1.6 以上の物体は無視されます。このフラグを有効にすると、その制限を解除します。車など横長の物体をトラッキングする場合に有効です。
    *
    * \~english
    * @def AILIA_TRACKER_FLAG_ALLOW_WIDE_ASPECT_RATIO
    * @brief By default, targets with an aspect ratio (width / height) greater than 1.6 are ignored in person tracking. Enabling this flag disables that restriction, making it suitable for tracking wide objects such as cars.
    */
    public const int AILIA_TRACKER_FLAG_ALLOW_WIDE_ASPECT_RATIO = (1);

    /****************************************************************
    * Tracking API
    **/

    /**
    * \~japanese
    * @brief トラッカーオブジェクトを作成します。
    * @param tracker AILIATracker構造体のポインタへのポインタ
    * @param algorithm AILIA_TRACKER_ALGORITHM_*
    * @param settings AILIATrackerSettings
    * @param version AILIA_TRACKER_SETTINGS_VERSION
    * @param flags AILIA_TRACKER_FLAG_*の論理和
    * @return
    *   成功した場合は \ref AILIA_STATUS_SUCCESS 、そうでなければエラーコードを返す。
    * @details
    *   AILIATrackerオブジェクトを作成します。
    *
    * \~english
    * @brief Creates a AILIATracker instance.
    * @param tracker A pointer to the AILIATracker instance pointer
    * @param algorithm AILIA_TRACKER_ALGORITHM_*
    * @param settings AILIATrackerSettings
    * @param version AILIA_TRACKER_SETTINGS_VERSION
    * @param flags Logical OR of AILIA_TRACKER_FLAG_*
    * @return
    *   If this function is successful, it returns  \ref AILIA_STATUS_SUCCESS , or an error code otherwise.
    * @details
    *   Creates a AILIATracker instance.
    */
    [DllImport(AiliaTracker.LIBRARY_NAME)]
    public static extern int ailiaTrackerCreate(ref IntPtr net, int algorithm, [In, Out] AILIATrackerSettings settings, int version, int flags);

    /**
    * \~japanese
    * @brief トラッキングの対象を登録します。
    * @param tracker トラッカーオブジェクトポインタ
    * @param detector_object トラッキング対象の物体へのポインター
    * @param version AILIA_DETECTOR_OBJECT_VERSION
    * @return
    *   成功した場合は \ref AILIA_STATUS_SUCCESS 、そうでなければエラーコードを返す。
    *
    * \~english
    * @brief Set tracking target
    * @param tracker An AILIATracker instance pointer
    * @param detector_object A pointer to the tracking target
    * @param version AILIA_DETECTOR_OBJECT_VERSION
    * @return
    *   If this function is successful, it returns  \ref AILIA_STATUS_SUCCESS , or an error code otherwise.
    */
    [DllImport(AiliaTracker.LIBRARY_NAME)]
    public static extern int ailiaTrackerAddTarget(IntPtr net, [In, Out] AiliaDetector.AILIADetectorObject detector_object, int version);

    /**
    * \~japanese
    * @brief トラッキングを行います。
    * @param tracker トラッカーオブジェクトポインタ
    * @return
    *   成功した場合は \ref AILIA_STATUS_SUCCESS 、そうでなければエラーコードを返す。
    * @details
    *   認識した結果はailiaTrackerGetObject APIで取得します。
    *
    * \~english
    * @brief Perform tracking
    * @param tracker An AILIATracker instance pointer
    * @return
    *   If this function is successful, it returns  \ref AILIA_STATUS_SUCCESS , or an error code otherwise.
    * @details
    *   Get the recognition result with ailiaTrackerGetObject API.
    */
    [DllImport(AiliaTracker.LIBRARY_NAME)]
    public static extern int ailiaTrackerCompute(IntPtr net);

    /**
    * \~japanese
    * @brief 検出結果の数を取得します。
    * @param tracker   トラッカーオブジェクトポインタ
    * @param obj_count  オブジェクト数
    * @return
    *   成功した場合は \ref AILIA_STATUS_SUCCESS 、そうでなければエラーコードを返す。
    *
    * \~english
    * @brief Gets the number of detection results.
    * @param tracker   An AILIATracker instance pointer
    * @param obj_count  The number of objects
    * @return
    *   If this function is successful, it returns  \ref AILIA_STATUS_SUCCESS , or an error code otherwise.
    */
    [DllImport(AiliaTracker.LIBRARY_NAME)]
    public static extern int ailiaTrackerGetObjectCount(IntPtr tracker, ref uint obj_count);

    /**
    * \~japanese
    * @brief 検出結果を取得します。
    * @param tracker   トラッカーオブジェクトポインタ
    * @param obj        オブジェクト情報
    * @param index      取得するオブジェクトのインデックス
    * @param version    AILIA_TRACKER_OBJECT_VERSION
    * @return
    *   成功した場合は \ref AILIA_STATUS_SUCCESS 、そうでなければエラーコードを返す。
    * @details
    *   ailiaTrackerCompute() を一度も実行していない場合は \ref AILIA_STATUS_INVALID_STATE が返ります。
    *   検出結果は推定確率順でソートされます。
    *
    * \~english
    * @brief Gets the detection results.
    * @param tracker   A AILIATracker instance pointer
    * @param obj        Object information
    * @param index      Object index
    * @param version    AILIA_TRACKER_OBJECT_VERSION
    * @return
    *   If this function is successful, it returns  \ref AILIA_STATUS_SUCCESS , or an error code otherwise.
    * @details
    *   If  ailiaTrackerCompute()  is not run at all, the function returns  \ref AILIA_STATUS_INVALID_STATE .
    *   The detection results are sorted in the order of estimated probability.
    */
    [DllImport(AiliaTracker.LIBRARY_NAME)]
    public static extern int ailiaTrackerGetObject(IntPtr tracker, [In, Out] AILIATrackerObject obj, uint index, uint version);

    /**
    * \~japanese
    * @brief トラッカーオブジェクトを破棄します。
    * @param tracker トラッカーオブジェクトポインタ
    * @return
    *   成功した場合は \ref AILIA_STATUS_SUCCESS 、そうでなければエラーコードを返す。
    *
    * \~english
    * @brief It destroys the AILIATracker instance.
    * @param tracker An AILIATracker instance pointer
    * @return
    *   If this function is successful, it returns  \ref AILIA_STATUS_SUCCESS , or an error code otherwise.
    */
    [DllImport(AiliaTracker.LIBRARY_NAME)]
    public static extern int ailiaTrackerDestroy(IntPtr net);

    /**
    * \~japanese
    * @brief エラーの詳細を返します
    * @param tracker  トラッカーオブジェクトポインタ
    * @return
    *   エラー詳細
    * @details
    *   返値は解放する必要はありません。
    *   文字列の有効期間は次にailiaTrackerのAPIを呼ぶまでです。
    *
    * \~english
    * @brief Returns the details of errors.
    * @param tracker   The AILIATracker instance pointer
    * @return
    *   Error details
    * @details
    *   The return value does not have to be released.
    *   The string is valid until the next ailiaTracker API function is called.
    */
    [DllImport(AiliaTracker.LIBRARY_NAME)]
    public static extern IntPtr ailiaTrackerGetErrorDetail(IntPtr net);
}
} // ailiaTracker