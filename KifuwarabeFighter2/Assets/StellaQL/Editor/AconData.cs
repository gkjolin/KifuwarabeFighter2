﻿//
// Animation Controller Tables
//
using System.Collections.Generic;
using System.Text;
using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;
using System;

namespace StellaQL
{
    public class AnimatorControllerWrapper
    {
        public AnimatorControllerWrapper(AnimatorController ac)
        {
            SourceAc = ac;

            // レイヤーのコピー
            CopiedLayers = new List<AnimatorControllerLayer>();
            foreach (AnimatorControllerLayer actualLayer in ac.layers) // オリジナルのレイヤーは、セッターが死んでる？
            {
                AnimatorControllerLayer copiedLayer = Operation_Layer.DeepCopy(actualLayer);
                CopiedLayers.Add(copiedLayer);
            }
        }

        public AnimatorController SourceAc { get; private set; }
        public List<AnimatorControllerLayer> CopiedLayers { get; private set; }
    }

    /// <summary>
    /// 列定義レコード
    /// </summary>
    public class RecordDefinition
    {
        public enum KeyType
        {
            /// <summary>
            /// StellaQLで使う、一時的なナンバリング
            /// </summary>
            TemporaryNumbering,
            /// <summary>
            /// Unityで識別子に使えそうなもの
            /// </summary>
            Identifiable,
            /// <summary>
            /// スプレッドシートで読取専用フィールドにしたい場合これ。
            /// </summary>
            ReadOnly,
            /// <summary>
            /// ユニティ・エディターが書込みに対応していない場合はこれ。
            /// </summary>
            UnityEditorDoesNotSupportWriting,
            None,
        }

        public enum FieldType
        {
            Int,
            Float,
            Bool,
            String,
            /// <summary>
            /// 特殊実装なもの。
            /// </summary>
            SpecialString,
            Other,//対応外
        }

        public RecordDefinition(string name, FieldType type, KeyType keyField, bool input)
        {
            this.Name = name;
            this.Type = type;
            this.KeyField = keyField;
            this.Input = input;
            this.m_getterBool = null;
            this.m_setterBool = null;
            this.m_getterFloat = null;
            this.m_setterFloat = null;
            this.m_getterInt = null;
            this.m_setterInt = null;
            this.m_getterString = null;
            this.m_setterString = null;
        }
        public RecordDefinition(string name, FieldType type, KeyType keyField, GettterBool getter, SetterBool setter)       : this(name, type, keyField, true)
        {
            this.m_getterBool = getter;     this.m_setterBool = setter;
        }
        public RecordDefinition(string name, FieldType type, KeyType keyField, GettterFloat getter, SetterFloat setter)     : this(name,type,keyField, true)
        {
            this.m_getterFloat = getter;    this.m_setterFloat = setter;
        }
        public RecordDefinition(string name, FieldType type, KeyType keyField, GettterInt getter, SetterInt setter)         : this(name, type, keyField, true)
        {
            this.m_getterInt = getter;      this.m_setterInt = setter;
        }
        public RecordDefinition(string name, FieldType type, KeyType keyField, GettterString getter, SetterString setter)   : this(name, type, keyField, true)
        {
            this.m_getterString = getter;   this.m_setterString = setter;
        }

        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 型
        /// </summary>
        public FieldType Type { get; private set; }

        /// <summary>
        /// キーとして利用できるフィールドか
        /// </summary>
        public KeyType KeyField { get; private set; }

        /// <summary>
        /// スプレッド・シートから入力可能か
        /// </summary>
        public bool Input { get; private set; }

        /// <summary>
        /// 列の記入漏れを防ぐためのものだぜ☆（＾～＾）
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="c">contents (コンテンツ)</param>
        /// <param name="n">output column name (列名出力)</param>
        /// <param name="d">output definition (列定義出力)</param>
        public void AppendCsv(Dictionary<string, object> fields, StringBuilder contents, bool outputColumnName, bool outputDefinition)
        {
            if (outputDefinition) // 列定義一列分を出力するなら
            {
                if (outputColumnName) // 列名を出力するなら
                {
                }
                else
                {
                    contents.Append(Name); contents.Append(",");
                    contents.Append(Type.ToString().Substring(0, 1).ToLower()); // 列挙型の要素名の先頭を小文字にして、型名とする。
                    contents.Append(Type.ToString().Substring(1));
                    contents.Append(",");
                    contents.Append(KeyField); contents.Append(",");
                    contents.Append(Input); contents.Append(",");
                    contents.AppendLine();
                }
            }
            else // 1フィールド分を出力するなら
            {
                if (outputColumnName) // 列名を出力するなら
                {
                    switch (this.Type)
                    {
                        case FieldType.Int://thru
                        case FieldType.Float:
                        case FieldType.Bool: contents.Append(this.Name); contents.Append(","); break;
                        case FieldType.String://thru
                        case FieldType.Other:
                        default: contents.Append(this.Name); contents.Append(","); break;
                    }
                }
                else
                {
                    switch (this.Type)
                    {
                        case FieldType.Int://thru
                        case FieldType.Float:
                        case FieldType.Bool: contents.Append(fields[Name]); contents.Append(","); break;
                        case FieldType.String://thru
                        case FieldType.Other:
                        default: contents.Append(CsvParser.EscapeCell((string)fields[Name])); contents.Append(","); break;
                    }
                }
            }
        }

        public static void AppendDefinitionHeader(StringBuilder contents)
        {
            contents.AppendLine("Name,Type,KeyField,Input,[EOL],"); // 列定義ヘッダー出力
        }

        public delegate bool   GettterBool  (object instance);                  GettterBool m_getterBool;
        public delegate void   SetterBool   (object instance, bool value);      SetterBool m_setterBool;
        public delegate float  GettterFloat (object instance);                  GettterFloat m_getterFloat;
        public delegate void   SetterFloat  (object instance, float value);     SetterFloat m_setterFloat;
        public delegate int    GettterInt   (object instance);                  GettterInt m_getterInt;
        public delegate void   SetterInt    (object instance, int value);       SetterInt m_setterInt;
        public delegate string GettterString(object instance);                  GettterString m_getterString;
        public delegate void   SetterString (object instance, string value);    SetterString m_setterString;

        public bool EqualsOld(object actualOld, object requestOld)
        {
            switch (Type)
            {
                case FieldType.Bool:    if ((bool)actualOld != (bool)requestOld) { throw new UnityException("Old値が異なる bool型 actual=[" + actualOld + "] old=[" + requestOld + "]"); } break;
                case FieldType.Float:   if ((float)actualOld != (float)requestOld) { throw new UnityException("Old値が異なる float型 actual=[" + actualOld + "] old=[" + requestOld + "]"); } break;
                case FieldType.Int:     if ((int)actualOld != (int)requestOld) { throw new UnityException("Old値が異なる int型 actual=[" + actualOld + "] old=[" + requestOld + "]"); } break;
                case FieldType.Other:   throw new UnityException("対応しないデータだぜ☆（＾～＾） FieldType=[" + Type.ToString() + "]"); // 未対応は、この型にしてあるんだぜ☆（＾▽＾）
                case FieldType.String:  if ((string)actualOld != (string)requestOld) { throw new UnityException("Old値が異なる string型 actual=[" + actualOld + "] old=[" + requestOld + "]"); } break;
                default: throw new UnityException("未定義の型だぜ☆（＾～＾） FieldType=[" + Type.ToString() + "]");
            }            
            return true;
        }

        /// <summary>
        /// 既存のオブジェクトのプロパティー更新の場合、これを使う。
        /// </summary>
        /// <param name="instance">ステートマシン、チャイルド・ステート、コンディション・ラッパー、ポジション・ラッパー等</param>
        /// <param name="record"></param>
        /// <param name="message"></param>
        public void Update(object instance, DataManipulationRecord record, StringBuilder message)
        {
            if (null == instance) { throw new UnityException("instanceがヌルだったぜ☆（／＿＼）"); }
            switch (Type) {
                case FieldType.Bool: {
                        if (null == m_getterBool) { throw new UnityException("m_getterBoolがヌルだったぜ☆（／＿＼）"); }
                        bool actual = m_getterBool(instance);
                        if (EqualsOld(actual, record.OldBool)) { m_setterBool(instance, record.NewBool); }
                    } break;
                case FieldType.Float: {
                        if (null == m_getterFloat) { throw new UnityException("m_getterFloatがヌルだったぜ☆（／＿＼）"); }
                        float actual = m_getterFloat(instance);
                        if (EqualsOld(actual, record.OldFloat)) { m_setterFloat(instance, record.NewFloat); }
                    } break;
                case FieldType.Int: {
                        if (null == m_getterInt) { throw new UnityException("m_getterIntがヌルだったぜ☆（／＿＼）"); }
                        int actual = m_getterInt(instance);
                        if (EqualsOld(actual, record.OldInt)) { m_setterInt(instance, record.NewInt); }
                    } break;
                case FieldType.Other: break; // 未対応は、この型にしてあるんだぜ☆（＾▽＾）
                case FieldType.String: {
                        Debug.Log("string型の更新要求だぜ☆（＾～＾）Name=["+Name+ "] record.IsDelete=["+ record.IsDelete + "] record.New=["+ record.New + "]");
                        if (null == m_getterString) { throw new UnityException("m_getterStringがヌルだったぜ☆（／＿＼）"); }
                        string actual = m_getterString(instance);
                        if (EqualsOld(actual, record.Old)) {
                            if (record.IsDelete) { m_setterString(instance, ""); } // 空文字列にセットする
                            else { m_setterString(instance, record.New); }
                        }
                    } break;
                default: throw new UnityException("未定義の型だぜ☆（＾～＾） FieldType=["+Type.ToString()+"]");
            }
        }
    }

    /// <summary>
    /// パラメーター
    /// FIXME: UnityEditorからはプロパティーの更新が反映されない？
    /// </summary>
    public class ParameterRecord
    {
        static ParameterRecord()
        {
            List<RecordDefinition> temp = new List<RecordDefinition>()
            {
                new RecordDefinition("num"          , RecordDefinition.FieldType.Int            ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("#name_ID#"    , RecordDefinition.FieldType.String         ,RecordDefinition.KeyType.Identifiable          ,false),
                new RecordDefinition("name"         , RecordDefinition.FieldType.String         ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{          return ((AnimatorControllerParameter)i).name; }
                    ,(object i,string v)=>{ ((AnimatorControllerParameter)i).name = v; }
                ),
                new RecordDefinition("#type_String#", RecordDefinition.FieldType.String         ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{          return ((AnimatorControllerParameter)i).type.ToString(); }
                    ,(object i,string v)=>{ ((AnimatorControllerParameter)i).type = (AnimatorControllerParameterType)Enum.Parse(typeof(AnimatorControllerParameterType),v); }
                ),
                new RecordDefinition("defaultBool"  , RecordDefinition.FieldType.Bool           ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{        return ((AnimatorControllerParameter)i).defaultBool; }
                    ,(object i,bool v)=>{ ((AnimatorControllerParameter)i).defaultBool = v; }
                ),
                new RecordDefinition("defaultFloat" , RecordDefinition.FieldType.Float          ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{         return ((AnimatorControllerParameter)i).defaultFloat; }
                    ,(object i,float v)=>{ ((AnimatorControllerParameter)i).defaultFloat = v; }
                ),
                new RecordDefinition("defaultInt"   , RecordDefinition.FieldType.Int            ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{       return ((AnimatorControllerParameter)i).defaultInt; }
                    ,(object i,int v)=>{ ((AnimatorControllerParameter)i).defaultInt = v; }
                ),
                new RecordDefinition("nameHash"     , RecordDefinition.FieldType.Int            ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{       return ((AnimatorControllerParameter)i).nameHash; }
                    ,(object i,int v)=>{ throw new UnityException("セットには未対応☆（＞＿＜）");}
                ),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }
            Empty = new ParameterRecord(-1, "", false, 0.0f, -1, 0, (AnimatorControllerParameterType)0);
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static ParameterRecord Empty { get; private set; }

        public ParameterRecord(int num, string name, bool numberBool, float numberFloat, int numberInt, int nameHash, AnimatorControllerParameterType type)
        {
            this.Fields = new Dictionary<string, object>()
            {
                { "num"             ,num                },
                { "#name_ID#"       ,name               }, // ID用
                { "name"            ,name               }, // 編集用
                { "#type_String#"   ,type.ToString()    },
                { "defaultBool"     ,numberBool         },
                { "defaultFloat"    ,numberFloat        },
                { "defaultInt"      ,numberInt          },
                { "nameHash"        ,nameHash           },
            };
        }
        public Dictionary<string, object> Fields { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">contents (コンテンツ)</param>
        /// <param name="n">output column name (列名出力)</param>
        /// <param name="d">output definition (列定義出力)</param>
        public void AppendCsvLine(StringBuilder c, bool n, bool d)
        {
            Definitions["num"           ].AppendCsv(Fields, c, n, d);
            Definitions["#name_ID#"     ].AppendCsv(Fields, c, n, d);
            Definitions["name"          ].AppendCsv(Fields, c, n, d);
            Definitions["#type_String#" ].AppendCsv(Fields, c, n, d);
            Definitions["defaultBool"   ].AppendCsv(Fields, c, n, d);
            Definitions["defaultFloat"  ].AppendCsv(Fields, c, n, d);
            Definitions["defaultInt"    ].AppendCsv(Fields, c, n, d);
            Definitions["nameHash"      ].AppendCsv(Fields, c, n, d);
            if (n) { c.Append("[EOL],"); }
            if (!d) { c.AppendLine(); }
        }
    }

    /// <summary>
    /// レイヤー
    /// </summary>
    public class LayerRecord
    {
        /// <summary>
        /// レイヤーのコピーを渡されても更新できないので、アニメーション・コントローラーも一緒に渡そうというもの。
        /// </summary>
        public class LayerWrapper
        {
            public LayerWrapper(AnimatorControllerWrapper sourceAcWrapper, int layerIndex)
            {
                SourceAcWrapper = sourceAcWrapper;
                LayerIndex = layerIndex;
            }

            public AnimatorControllerWrapper SourceAcWrapper { get; private set; }
            public int LayerIndex { get; private set; }
        }

        static LayerRecord()
        {
            List<RecordDefinition> temp = new List<RecordDefinition>()
            {
                // #で囲んでいるのは、StellaQL用のフィールド。文字列検索しやすいように単語を # で挟んでいる。
                new RecordDefinition("#layerNum#"               ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("name"                     ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable          ,false),
                new RecordDefinition("#avatarMask_assetPath#"   ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{
                        if(null==((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].avatarMask) { Debug.Log("アバターマスク無し☆（＞＿＜）"); return ""; }
                        return AssetDatabase.GetAssetPath(((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].avatarMask.GetInstanceID());
                    }
                    ,(object i,string v)=>{
                        AvatarMask value = AssetDatabase.LoadAssetAtPath<AvatarMask>(v);
                        if(null==value) { throw new UnityException("["+v+"]というアセット・パスのアバターマスクは見つからなかったぜ☆（＞＿＜）！"); }
                        ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].avatarMask = value;
                        Debug.Log("アバターマスクアセットパス（＾～＾）◆v=["+v+"] layerIndex=["+((LayerWrapper)i).LayerIndex+"]");
                        Operation_Layer.DumpLog(((LayerWrapper)i).SourceAcWrapper);
                        // TODO: Delete にも対応したい。
                    }),
                new RecordDefinition("#blendingMode_string#"           ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{ return ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].blendingMode.ToString(); }
                    ,(object i,string v)=>{
                        HashSet<AnimatorLayerBlendingMode> hits = Operation_AnimatorLayerBlendingMode.Fetch(v);
                        if(0==hits.Count) { throw new UnityException("正規表現に該当する列挙型の要素が無いぜ☆（＞＿＜）！ v=["+v+"] hits.Count=["+hits.Count+"]"); }
                        else if(1<hits.Count)
                        {
                            StringBuilder sb = new StringBuilder(); foreach(AnimatorLayerBlendingMode enumItem in hits) { sb.Append(enumItem.ToString()); sb.Append(" "); }
                            throw new UnityException("正規表現に該当する列挙型の要素はどれか１つにしろだぜ☆（＞＿＜）！ v=["+v+"] hits.Count=["+hits.Count+"] sb="+sb.ToString());
                        }
                        AnimatorLayerBlendingMode value = 0; bool found =false;
                        foreach(AnimatorLayerBlendingMode enumItem in hits) { value = enumItem; found=true; break; }
                        if(found) {
                            ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].blendingMode = value;
                            Debug.Log("ブレンドモードストリング（＾～＾）◆v=["+v+"] layerIndex=["+((LayerWrapper)i).LayerIndex+"]");
                            Operation_Layer.DumpLog(((LayerWrapper)i).SourceAcWrapper);
                        }
                    }),
                new RecordDefinition("defaultWeight"            ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{ return ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].defaultWeight;           }
                    ,(object i,float v)=>{
                        ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].defaultWeight = v;
                        Debug.Log("デフォルトウェイト（＾～＾）◆v=["+v+"] layerIndex=["+((LayerWrapper)i).LayerIndex+"]");
                        Operation_Layer.DumpLog(((LayerWrapper)i).SourceAcWrapper);
                    }),
                new RecordDefinition("iKPass"                   ,RecordDefinition.FieldType.Bool    ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{ return ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].iKPass;                  }
                    ,(object i,bool v)=>{
                        ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].iKPass = v;
                        Debug.Log("アイケーパス（＾～＾）◆v=["+v+"] layerIndex=["+((LayerWrapper)i).LayerIndex+"]");
                        Operation_Layer.DumpLog(((LayerWrapper)i).SourceAcWrapper);
                    }),
                new RecordDefinition("syncedLayerAffectsTiming" ,RecordDefinition.FieldType.Bool    ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{ return ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].syncedLayerAffectsTiming;}
                    ,(object i,bool v)=>{
                        ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].syncedLayerAffectsTiming = v;
                        Debug.Log("シンクレイヤーアフェクトタイミング（＾～＾）◆v=["+v+"] layerIndex=["+((LayerWrapper)i).LayerIndex+"]");
                        Operation_Layer.DumpLog(((LayerWrapper)i).SourceAcWrapper);
                    }),
                new RecordDefinition("syncedLayerIndex"         ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{ return ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].syncedLayerIndex;        }
                    ,(object i,int v)=>{
                        ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].syncedLayerIndex = v;
                        Debug.Log("シンクレイヤーインデックス（＾～＾）◆v=["+v+"] layerIndex=["+((LayerWrapper)i).LayerIndex+"]");
                        Operation_Layer.DumpLog(((LayerWrapper)i).SourceAcWrapper);
                    }),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }
            Empty = new LayerRecord(-1, new AnimatorControllerLayer());
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static LayerRecord Empty { get; private set; }

        public LayerRecord(int num, AnimatorControllerLayer layer)
        {
            this.Fields = new Dictionary<string, object>()
            {
                { "#layerNum#",num },//レイヤー行番号
                { "name"                        ,layer.name},//レイヤー名
                { "#avatarMask_assetPath#"      ,layer.avatarMask == null ? "" : AssetDatabase.GetAssetPath(layer.avatarMask.GetInstanceID()) },
                { "#blendingMode_string#"       ,layer.blendingMode.ToString()},
                { "defaultWeight"               ,layer.defaultWeight},
                { "iKPass"                      ,layer.iKPass},
                { "syncedLayerAffectsTiming"    ,layer.syncedLayerAffectsTiming},
                { "syncedLayerIndex"            ,layer.syncedLayerIndex},
            };
        }
        public Dictionary<string,object> Fields { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">contents (コンテンツ)</param>
        /// <param name="n">output column name (列名出力)</param>
        /// <param name="d">output definition (列定義出力)</param>
        public void AppendCsvLine(StringBuilder c, bool n, bool d)
        {
            Definitions["#layerNum#"                ].AppendCsv(Fields, c, n, d); // レイヤー行番号
            Definitions["name"                      ].AppendCsv(Fields, c, n, d); // レイヤー名
            Definitions["#avatarMask_assetPath#"    ].AppendCsv(Fields, c, n, d);
            Definitions["#blendingMode_string#"     ].AppendCsv(Fields, c, n, d);
            Definitions["defaultWeight"             ].AppendCsv(Fields, c, n, d);
            Definitions["iKPass"                    ].AppendCsv(Fields, c, n, d);
            Definitions["syncedLayerAffectsTiming"  ].AppendCsv(Fields, c, n, d);
            Definitions["syncedLayerIndex"          ].AppendCsv(Fields, c, n, d);
            if (n) { c.Append("[EOL],"); }
            if (!d) { c.AppendLine(); }
        }
    }

    /// <summary>
    /// ステートマシン
    /// </summary>
    public class StatemachineRecord
    {
        public class Wrapper {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="source"></param>
            /// <param name="statemachinePath">例えばフルパスが "Base Layer.Alpaca.Bear.Cat.Dog" のとき、"Alpaca.Bear.Cat"。</param>
            public Wrapper(AnimatorStateMachine source, string statemachinePath)
            {
                Source = source;
                StatemachinePath = statemachinePath;
            }

            public AnimatorStateMachine Source { get; private set; }
            public string StatemachinePath { get; private set; }
        }

        static StatemachineRecord()
        {
            List<RecordDefinition> temp = new List<RecordDefinition>()
            {
                // #で囲んでいるのは、StellaQL用のフィールド。文字列検索しやすいように単語を # で挟んでいる。
                new RecordDefinition("#layerNum#"                   ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#machineStateNum#"            ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#layerName#"                  ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable      ,false), // 内容は空。 LibreOffice Basic に探させる
                new RecordDefinition("#statemachinePath#"           ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable      // "Base Layer.Alpaca.Bear.Cat.Dog" のとき、"Alpaca.Bear.Cat"。
                    ,(object i)=>{          return ((Wrapper)i).StatemachinePath; }
                    ,(object i,string v)=>{ throw new UnityException("セットには未対応☆（＞＿＜）");}
                ),
                // new RecordDefinition("#fullnameEndsWithDot#"        ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable      ,false), // 表示用フィールド（フルネーム）はこれで十分。後ろにドットが付いているが……。
                new RecordDefinition("name"                         ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.ReadOnly          // 「#fullnameEndsWithDot#」フィールドで十分なので、これはIDにしない方が一貫性がある。
                    ,(object i)=>{          return ((Wrapper)i).Source.name; }
                    ,(object i,string v)=>{ throw new UnityException("セットには未対応☆（＞＿＜）");}
                ),
                new RecordDefinition("#anyStateTransitions_Length#" ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{          return ((Wrapper)i).Source.anyStateTransitions.Length.ToString(); }
                    ,(object i,string v)=>{ throw new UnityException("セットには未対応☆（＞＿＜）");}
                ),
                new RecordDefinition("#behaviours_Length#"          ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{          return ((Wrapper)i).Source.behaviours.Length.ToString(); }
                    ,(object i,string v)=>{ throw new UnityException("セットには未対応☆（＞＿＜）");}
                ),
                new RecordDefinition("#defaultState_String#"        ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{          return (((Wrapper)i).Source.defaultState==null) ? "" : ((Wrapper)i).Source.defaultState.ToString(); }
                    ,(object i,string v)=>{ throw new UnityException("セットには未対応☆（＞＿＜）");}
                ),
                new RecordDefinition("#entryTransitions_Length#"    ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{          return ((Wrapper)i).Source.entryTransitions.Length.ToString(); }
                    ,(object i,string v)=>{ throw new UnityException("セットには未対応☆（＞＿＜）");}
                ),
                new RecordDefinition("hideFlags"                    ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.None
                    ,(object i)=>{          return ((Wrapper)i).Source.hideFlags.ToString(); }
                    ,(object i,string v)=>{ ((Wrapper)i).Source.hideFlags = (HideFlags)System.Enum.Parse(typeof(HideFlags), v);}
                ),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }

            Empty = new StatemachineRecord(-1,-1,"",new AnimatorStateMachine(),new List<PositionRecord>());
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static StatemachineRecord Empty { get; private set; }

        public StatemachineRecord(int layerNum, int machineStateNum, string statemachinePath, AnimatorStateMachine stateMachine, List<PositionRecord> positionsTable)
        {
            //, string fullnameEndsWithDot

            // fullnameEndsWithDot には「Base Layer.」のような文字が入っているが、レイヤーの持つステートマシンの名前も「Base Layer」なので、つなげると「Base Layer.Base Layer」になってしまう。
            // state から見れば親パスだが。

            this.Fields = new Dictionary<string, object>()
            {
                { "#layerNum#"                      ,layerNum                                                                                           },//レイヤー行番号
                { "#machineStateNum#"               ,machineStateNum                                                                                    },
                { "#layerName#"                     ,""                                                                                                 },//空っぽ
                { "#statemachinePath#"              ,statemachinePath                                                                                   },
                { "name"                            ,stateMachine.name                                                                                  },
                { "#anyStateTransitions_Length#"    ,stateMachine.anyStateTransitions.Length.ToString()                                                 },
                { "#behaviours_Length#"             ,stateMachine.behaviours.Length.ToString()                                                          },
                { "#defaultState_String#"           ,stateMachine.defaultState          == null ? "" : stateMachine.defaultState.ToString()             },
                { "#entryTransitions_Length#"       ,stateMachine.entryTransitions.Length.ToString()                                                    },
                { "hideFlags"                       ,stateMachine.hideFlags.ToString()                                                                  },
            };

            if (stateMachine.anyStatePosition           != null) { positionsTable.Add(new PositionRecord(layerNum, machineStateNum, -1, -1, -1, "anyStatePosition"              ,stateMachine.anyStatePosition          )); }
            if (stateMachine.entryPosition              != null) { positionsTable.Add(new PositionRecord(layerNum, machineStateNum, -1, -1, -1, "entryPosition"                 ,stateMachine.entryPosition             )); }
            if (stateMachine.exitPosition               != null) { positionsTable.Add(new PositionRecord(layerNum, machineStateNum, -1, -1, -1, "exitPosition"                  ,stateMachine.exitPosition              )); }
            if (stateMachine.parentStateMachinePosition != null) { positionsTable.Add(new PositionRecord(layerNum, machineStateNum, -1, -1, -1, "parentStateMachinePosition"    ,stateMachine.parentStateMachinePosition)); }
        }
        public Dictionary<string, object> Fields { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">contents (コンテンツ)</param>
        /// <param name="n">output column name (列名出力)</param>
        /// <param name="d">output definition (列定義出力)</param>
        public void AppendCsvLine(StringBuilder c, bool n, bool d)
        {
            Definitions["#layerNum#"                    ].AppendCsv(Fields, c, n, d); // レイヤー行番号
            Definitions["#machineStateNum#"             ].AppendCsv(Fields, c, n, d);
            Definitions["#layerName#"                   ].AppendCsv(Fields, c, n, d);
            Definitions["#statemachinePath#"            ].AppendCsv(Fields, c, n, d);
            Definitions["name"                          ].AppendCsv(Fields, c, n, d);
            Definitions["#anyStateTransitions_Length#"  ].AppendCsv(Fields, c, n, d);
            Definitions["#behaviours_Length#"           ].AppendCsv(Fields, c, n, d);
            Definitions["#defaultState_String#"         ].AppendCsv(Fields, c, n, d);
            Definitions["#entryTransitions_Length#"     ].AppendCsv(Fields, c, n, d);
            Definitions["hideFlags"                     ].AppendCsv(Fields, c, n, d);
            if (n) { c.Append("[EOL],"); }
            if (!d) { c.AppendLine(); }
        }
    }

    /// <summary>
    /// ステート
    /// </summary>
    public class StateRecord
    {
        public class Wrapper
        {
            public Wrapper(AnimatorState source)//, string statemachinePath
            {
                Source = source;
                //StatemachinePath = statemachinePath;
            }

            public AnimatorState Source { get; private set; }
            //public string StatemachinePath { get; private set; }
        }

        static StateRecord()
        {
            List<RecordDefinition> temp = new List<RecordDefinition>()
            {
                new RecordDefinition("#layerNum#"                   ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#machineStateNum#"            ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#stateNum#"                   ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#layerName#"                  ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable      ,false), // 内容は空。 LibreOffice Basic に探させる
                new RecordDefinition("#statemachinePath#"           ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable      ,false), // 内容は空。 LibreOffice Basic に探させる
                new RecordDefinition("name"                         ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable  ,false) ,
                new RecordDefinition("cycleOffset"                  ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.cycleOffset; }         ,(object i,float v)=>{ ((Wrapper)i).Source.cycleOffset = v; }),
                new RecordDefinition("cycleOffsetParameter"         ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.cycleOffsetParameter; },(object i,string v)=>{ ((Wrapper)i).Source.cycleOffsetParameter = v; }),
                new RecordDefinition("hideFlags"                    ,RecordDefinition.FieldType.Other   ,RecordDefinition.KeyType.None          ,false),
                new RecordDefinition("iKOnFeet"                     ,RecordDefinition.FieldType.Bool    ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.iKOnFeet; }            ,(object i,bool v)=>{ ((Wrapper)i).Source.iKOnFeet = v; }),
                new RecordDefinition("mirror"                       ,RecordDefinition.FieldType.Bool    ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.mirror; }              ,(object i,bool v)=>{ ((Wrapper)i).Source.mirror = v; }),
                new RecordDefinition("mirrorParameter"              ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.mirrorParameter; }     ,(object i,string v)=>{ ((Wrapper)i).Source.mirrorParameter = v; }),
                new RecordDefinition("mirrorParameterActive"        ,RecordDefinition.FieldType.Bool    ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.mirrorParameterActive;},(object i,bool v)=>{ ((Wrapper)i).Source.mirrorParameterActive = v; }),
                new RecordDefinition("motion_name"                  ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.None          ,false),
                new RecordDefinition("nameHash"                     ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.None          ,false),
                new RecordDefinition("speed"                        ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.speed; }               ,(object i,float v)=>{ ((Wrapper)i).Source.speed = v; }),
                new RecordDefinition("speedParameter"               ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.speedParameter; }      ,(object i,string v)=>{ ((Wrapper)i).Source.speedParameter = v; }),
                new RecordDefinition("speedParameterActive"         ,RecordDefinition.FieldType.Bool    ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.speedParameterActive; },(object i,bool v)=>{ ((Wrapper)i).Source.speedParameterActive = v; }),
                new RecordDefinition("tag"                          ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.tag; }                 ,(object i,string v)=>{ ((Wrapper)i).Source.tag = v; }),
                new RecordDefinition("writeDefaultValues"           ,RecordDefinition.FieldType.Bool    ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.writeDefaultValues; }  ,(object i,bool v)=>{ ((Wrapper)i).Source.writeDefaultValues = v; }),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }

            Empty = new StateRecord(-1,-1,-1,new AnimatorState());
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static StateRecord Empty { get; private set; }

        public static StateRecord CreateInstance(int layerNum, int machineStateNum, int stateNum, string parentPath, ChildAnimatorState caState, List<PositionRecord> positionsTable)
        {
            //, string statemachinePath
            positionsTable.Add(new PositionRecord(layerNum, machineStateNum, stateNum, -1, -1, "position", caState.position));
            return new StateRecord(layerNum, machineStateNum, stateNum, caState.state);
            //, statemachinePath
        }
        public StateRecord(int layerNum, int machineStateNum, int stateNum, AnimatorState state)
        {
            //, string statemachinePath
            this.Fields = new Dictionary<string, object>()
            {
                { "#layerNum#",layerNum },//レイヤー行番号
                { "#machineStateNum#", machineStateNum},
                { "#stateNum#",stateNum },
                { "#layerName#", ""},//空っぽ
                { "#statemachinePath#", ""},//空っぽ
                { "name", state.name},
                { "cycleOffset", state.cycleOffset},
                { "cycleOffsetParameter", state.cycleOffsetParameter},
                { "hideFlags", state.hideFlags.ToString()},
                { "iKOnFeet", state.iKOnFeet},
                { "mirror", state.mirror},
                { "mirrorParameter",state.mirrorParameter },
                { "mirrorParameterActive",state.mirrorParameterActive },
                { "motion_name", state.motion == null ? "" : state.motion.name},// とりあえず名前だけ☆
                { "nameHash", state.nameHash},// このハッシュは有効なのだろうか？
                { "speed", state.speed},
                { "speedParameter", state.speedParameter},
                { "speedParameterActive", state.speedParameterActive},
                { "tag", state.tag},
                { "writeDefaultValues", state.writeDefaultValues},
            };
        }
        public Dictionary<string, object> Fields { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">contents (コンテンツ)</param>
        /// <param name="n">output column name (列名出力)</param>
        /// <param name="d">output definition (列定義出力)</param>
        public void AppendCsvLine(StringBuilder c, bool n, bool d)
        {
            Definitions["#layerNum#"].AppendCsv(Fields, c, n, d); // レイヤー行番号
            Definitions["#machineStateNum#"].AppendCsv(Fields, c, n, d);
            Definitions["#stateNum#"].AppendCsv(Fields, c, n, d);
            Definitions["#layerName#"].AppendCsv(Fields, c, n, d);
            Definitions["#statemachinePath#"].AppendCsv(Fields, c, n, d);
            Definitions["name"].AppendCsv(Fields, c, n, d);
            Definitions["cycleOffset"].AppendCsv(Fields, c, n, d);
            Definitions["cycleOffsetParameter"].AppendCsv(Fields, c, n, d);
            Definitions["hideFlags"].AppendCsv(Fields, c, n, d);
            Definitions["iKOnFeet"].AppendCsv(Fields, c, n, d);
            Definitions["mirror"].AppendCsv(Fields, c, n, d);
            Definitions["mirrorParameter"].AppendCsv(Fields, c, n, d);
            Definitions["mirrorParameterActive"].AppendCsv(Fields, c, n, d);
            Definitions["motion_name"].AppendCsv(Fields, c, n, d);
            Definitions["nameHash"].AppendCsv(Fields, c, n, d);
            Definitions["speed"].AppendCsv(Fields, c, n, d);
            Definitions["speedParameter"].AppendCsv(Fields, c, n, d);
            Definitions["speedParameterActive"].AppendCsv(Fields, c, n, d);
            Definitions["tag"].AppendCsv(Fields, c, n, d);
            Definitions["writeDefaultValues"].AppendCsv(Fields, c, n, d);
            if (n) { c.Append("[EOL],"); }
            if (!d) { c.AppendLine(); }
        }
    }

    /// <summary>
    /// トランジション
    /// ※コンディションは別テーブル
    /// </summary>
    public class TransitionRecord
    {
        static TransitionRecord()
        {
            List<RecordDefinition> temp = new List<RecordDefinition>()
            {
                new RecordDefinition("#layerNum#"                       ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#machineStateNum#"                ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#stateNum#"                       ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#transitionNum#"                  ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#layerName#"                      ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable      ,false),
                new RecordDefinition("#statemachinePath#"               ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable      ,false),
                new RecordDefinition("#stateName#"                      ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable      ,false),
                new RecordDefinition("name"                             ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable      ,false),
                new RecordDefinition("#stellaQLComment#"                ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.None              ,false),
                new RecordDefinition("canTransitionToSelf"              ,RecordDefinition.FieldType.Bool    ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).canTransitionToSelf; }   ,(object i,bool v)=>{ ((AnimatorStateTransition)i).canTransitionToSelf = v; }),
                new RecordDefinition("#destinationState_name#"          ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.None              ,false),
                new RecordDefinition("#destinationState_nameHash#"      ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.None              ,false),
                new RecordDefinition("#destinationStateMachine_name#"   ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.None              ,false),
                new RecordDefinition("duration"                         ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).duration; }              ,(object i,float v)=>{ ((AnimatorStateTransition)i).duration = v; }),
                new RecordDefinition("exitTime"                         ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).exitTime; }              ,(object i,float v)=>{ ((AnimatorStateTransition)i).exitTime = v; }),
                new RecordDefinition("hasExitTime"                      ,RecordDefinition.FieldType.Bool    ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).hasExitTime; }           ,(object i,bool v)=>{ ((AnimatorStateTransition)i).hasExitTime = v; }),
                new RecordDefinition("hasFixedDuration"                 ,RecordDefinition.FieldType.Bool    ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).hasFixedDuration; }      ,(object i,bool v)=>{ ((AnimatorStateTransition)i).hasFixedDuration = v; }),
                new RecordDefinition("hideFlags"                        ,RecordDefinition.FieldType.Other   ,RecordDefinition.KeyType.None              ,false),
                new RecordDefinition("interruptionSource"               ,RecordDefinition.FieldType.Other   ,RecordDefinition.KeyType.None              ,false),
                new RecordDefinition("isExit"                           ,RecordDefinition.FieldType.Bool    ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).isExit; }                ,(object i,bool v)=>{ ((AnimatorStateTransition)i).isExit = v; }),
                new RecordDefinition("mute"                             ,RecordDefinition.FieldType.Bool    ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).mute; }                  ,(object i,bool v)=>{ ((AnimatorStateTransition)i).mute = v; }),
                new RecordDefinition("offset"                           ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).offset; }                ,(object i,float v)=>{ ((AnimatorStateTransition)i).offset = v; }),
                new RecordDefinition("orderedInterruption"              ,RecordDefinition.FieldType.Bool    ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).orderedInterruption; }   ,(object i,bool v)=>{ ((AnimatorStateTransition)i).orderedInterruption = v; }),
                new RecordDefinition("solo"                             ,RecordDefinition.FieldType.Bool    ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).solo; }                  ,(object i,bool v)=>{ ((AnimatorStateTransition)i).solo = v; }),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }

            Empty = new TransitionRecord(-1,-1,-1,-1,new AnimatorStateTransition(),"");
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static TransitionRecord Empty { get; private set; }

        public TransitionRecord(int layerNum, int machineStateNum, int stateNum, int transitionNum, AnimatorStateTransition transition, string stellaQLComment)
        {
            this.Fields = new Dictionary<string, object>()
            {
                { "#layerNum#"                      , layerNum                                      },
                { "#machineStateNum#"               , machineStateNum                               },
                { "#stateNum#"                      , stateNum                                      },
                { "#transitionNum#"                 , transitionNum                                 },
                { "#layerName#"                     , ""                                            },
                { "#statemachinePath#"              , ""                                            },
                { "#stateName#"                     , ""                                            },
                { "name"                            , transition.name                               },
                { "#stellaQLComment#"               , stellaQLComment                               }, // どこからどこへ繋いでるのか、動作確認するために見る開発時用の欄を追加。
                { "canTransitionToSelf"             , transition.canTransitionToSelf                },
                { "#destinationState_name#"         , transition.destinationState == null ? "" : transition.destinationState.name},// 名前のみ取得
                { "#destinationState_nameHash#"     , transition.destinationState == null ? 0 : transition.destinationState.nameHash},
                { "#destinationStateMachine_name#"  , transition.destinationStateMachine == null ? "" : transition.destinationStateMachine.name},// 名前のみ取得
                { "duration"                        , transition.duration                           },
                { "exitTime"                        , transition.exitTime                           },
                { "hasExitTime"                     , transition.hasExitTime                        },
                { "hasFixedDuration"                , transition.hasFixedDuration                   },
                { "hideFlags"                       , transition.hideFlags.ToString()               },
                { "interruptionSource"              , transition.interruptionSource.ToString()      },
                { "isExit"                          , transition.isExit                             },
                { "mute"                            , transition.mute                               },
                { "offset"                          , transition.offset                             },
                { "orderedInterruption"             , transition.orderedInterruption                },
                { "solo"                            , transition.solo                               },
            };
        }
        public Dictionary<string, object> Fields { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">contents (コンテンツ)</param>
        /// <param name="n">output column name (列名出力)</param>
        /// <param name="d">output definition (列定義出力)</param>
        public void AppendCsvLine(StringBuilder c, bool n, bool d)
        {
            Definitions["#layerNum#"                        ].AppendCsv(Fields, c, n, d);
            Definitions["#machineStateNum#"                 ].AppendCsv(Fields, c, n, d);
            Definitions["#stateNum#"                        ].AppendCsv(Fields, c, n, d);
            Definitions["#transitionNum#"                   ].AppendCsv(Fields, c, n, d);
            Definitions["#layerName#"                       ].AppendCsv(Fields, c, n, d);
            Definitions["#statemachinePath#"                ].AppendCsv(Fields, c, n, d);
            Definitions["#stateName#"                       ].AppendCsv(Fields, c, n, d);
            Definitions["name"                              ].AppendCsv(Fields, c, n, d);
            Definitions["#stellaQLComment#"                 ].AppendCsv(Fields, c, n, d);
            Definitions["canTransitionToSelf"               ].AppendCsv(Fields, c, n, d);
            Definitions["#destinationState_name#"           ].AppendCsv(Fields, c, n, d);
            Definitions["#destinationState_nameHash#"       ].AppendCsv(Fields, c, n, d);
            Definitions["#destinationStateMachine_name#"    ].AppendCsv(Fields, c, n, d);
            Definitions["duration"                          ].AppendCsv(Fields, c, n, d);
            Definitions["exitTime"                          ].AppendCsv(Fields, c, n, d);
            Definitions["hasExitTime"                       ].AppendCsv(Fields, c, n, d);
            Definitions["hasFixedDuration"                  ].AppendCsv(Fields, c, n, d);
            Definitions["hideFlags"                         ].AppendCsv(Fields, c, n, d);
            Definitions["interruptionSource"                ].AppendCsv(Fields, c, n, d);
            Definitions["isExit"                            ].AppendCsv(Fields, c, n, d);
            Definitions["mute"                              ].AppendCsv(Fields, c, n, d);
            Definitions["offset"                            ].AppendCsv(Fields, c, n, d);
            Definitions["orderedInterruption"               ].AppendCsv(Fields, c, n, d);
            Definitions["solo"                              ].AppendCsv(Fields, c, n, d);
            if (n) { c.Append("[EOL],"); }
            if (!d) { c.AppendLine(); }
        }
    }

    /// <summary>
    /// コンディション
    /// </summary>
    public class ConditionRecord
    {
        /// <summary>
        /// struct を object に渡したいときに使うラッパーだぜ☆（＾～＾）
        /// </summary>
        public class AnimatorConditionWrapper
        {
            /// <summary>
            /// 空コンストラクタで生成した場合、.IsNull( ) メソッドでヌルを返す。
            /// </summary>
            public AnimatorConditionWrapper()
            {
                this.IsNull = true;
            }

            public AnimatorConditionWrapper(AnimatorCondition source)
            {
                this.m_source = source;
                this.IsNull = false;
            }

            public bool IsNull { get; private set; }
            public AnimatorCondition m_source;
        }

        /// <summary>
        /// モードには、数値型のときは演算子が入っているし、論理値型のときは論理値が入っている。
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static string Mode_to_string(AnimatorConditionMode mode)
        {
            if (0 == mode) { return ""; } // 0 もある。

            switch (mode)
            {
                case AnimatorConditionMode.Greater: return ">";
                case AnimatorConditionMode.Less: return "<";
                case AnimatorConditionMode.Equals: return "=";
                case AnimatorConditionMode.NotEqual: return "<>";
                case AnimatorConditionMode.If: return "TRUE";
                case AnimatorConditionMode.IfNot: return "FALSE";
                default: throw new UnityException("コンディションの未対応の演算子だったんだぜ☆（＞＿＜）[" + mode + "]");
            }
        }
        /// <summary>
        /// モードには、数値型のときは演算子が入っているし、論理値型のときは論理値が入っている。
        /// </summary>
        /// <param name="modeString"></param>
        /// <returns></returns>
        public static AnimatorConditionMode String_to_mode(string modeString)
        {
            if ("" == modeString) { return 0; } // 0 もある。

            switch (modeString.Trim().ToUpper())
            {
                case ">": return AnimatorConditionMode.Greater;
                case "<": return AnimatorConditionMode.Less;
                case "=": return AnimatorConditionMode.Equals;
                case "<>": return AnimatorConditionMode.NotEqual;
                case "TRUE": return AnimatorConditionMode.If;
                case "FALSE": return AnimatorConditionMode.IfNot;
                default: throw new UnityException("コンディションの未定義の演算子だったんだぜ☆（＞＿＜）[" + modeString + "]");
            }
        }

        static ConditionRecord()
        {
            List<RecordDefinition> temp = new List<RecordDefinition>()
            {
                new RecordDefinition("#layerNum#"           ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering    ,false  ),
                new RecordDefinition("#machineStateNum#"    ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering    ,false  ),
                new RecordDefinition("#stateNum#"           ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering    ,false  ),
                new RecordDefinition("#transitionNum#"      ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering    ,false  ),
                new RecordDefinition("#conditionNum#"       ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering    ,false  ),
                new RecordDefinition("#layerName#"          ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable          ,false  ),
                new RecordDefinition("#statemachinePath#"   ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable          ,false  ),
                new RecordDefinition("#stateName#"          ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable          ,false  ),

                // parameter, mode, threshold の順に並べた方が、理解しやすい。
                new RecordDefinition("parameter"            ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((AnimatorConditionWrapper)i).m_source.parameter; } ,(object i,string v)=>{ ((AnimatorConditionWrapper)i).m_source.parameter = v; }),
                // 演算子。本来はイニューム型だが、文字列型にする。
                // 値は本来は Greater,less,Equals,NotEqual,If,IfNot の６つだが、分かりづらいので >, <, =, <>, TRUE, FALSE の６つにする。
                new RecordDefinition("mode"                 ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.None
                    ,(object i)=>{ return Mode_to_string(((AnimatorConditionWrapper)i).m_source.mode);}
                    ,(object i,string v)=>{((AnimatorConditionWrapper)i).m_source.mode = String_to_mode(v);}
                ),
                new RecordDefinition("threshold"            ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((AnimatorConditionWrapper)i).m_source.threshold; } ,(object i,float v)=>{ ((AnimatorConditionWrapper)i).m_source.threshold = v; }),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }

            Empty = new ConditionRecord(-1, -1, -1, -1, -1, new AnimatorCondition());
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static ConditionRecord Empty { get; set; }

        public ConditionRecord(int layerNum, int machineStateNum, int stateNum, int transitionNum, int conditionNum, AnimatorCondition condition)
        {
            this.Fields = new Dictionary<string, object>()
            {
                { "#layerNum#"          , layerNum},
                { "#machineStateNum#"   , machineStateNum},
                { "#stateNum#"          , stateNum},
                { "#transitionNum#"     , transitionNum},
                { "#conditionNum#"      , conditionNum},
                { "#layerName#"         , ""                                            },
                { "#statemachinePath#"  , ""                                            },
                { "#stateName#"         , ""                                            },
                { "mode"                , Mode_to_string( condition.mode)}, // 内容を変えて入れる。
                { "parameter"           , condition.parameter},
                { "threshold"           , condition.threshold},
            };
        }
        public Dictionary<string, object> Fields { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">contents (コンテンツ)</param>
        /// <param name="n">output column name (列名出力)</param>
        /// <param name="d">output definition (列定義出力)</param>
        public void AppendCsvLine(StringBuilder c, bool n, bool d)
        {
            Definitions["#layerNum#"            ].AppendCsv(Fields, c, n, d);
            Definitions["#machineStateNum#"     ].AppendCsv(Fields, c, n, d);
            Definitions["#stateNum#"            ].AppendCsv(Fields, c, n, d);
            Definitions["#transitionNum#"       ].AppendCsv(Fields, c, n, d);
            Definitions["#conditionNum#"        ].AppendCsv(Fields, c, n, d);
            Definitions["#layerName#"           ].AppendCsv(Fields, c, n, d);
            Definitions["#statemachinePath#"    ].AppendCsv(Fields, c, n, d);
            Definitions["#stateName#"           ].AppendCsv(Fields, c, n, d);
            Definitions["mode"                  ].AppendCsv(Fields, c, n, d);
            Definitions["parameter"             ].AppendCsv(Fields, c, n, d);
            Definitions["threshold"             ].AppendCsv(Fields, c, n, d);
            if (n) { c.Append("[EOL],"); }
            if (!d) { c.AppendLine(); }
        }
    }

    /// <summary>
    /// ポジション
    /// </summary>
    public class PositionRecord
    {
        /// <summary>
        /// struct を object に渡したいときに使うラッパーだぜ☆（＾～＾）
        /// </summary>
        public class PositionWrapper
        {
            public PositionWrapper(AnimatorStateMachine statemachine, string propertyName)
            {
                this.m_statemachine = statemachine;
                this.PropertyName = propertyName;
            }
            public PositionWrapper(ChildAnimatorState caState, string propertyName)
            {
                this.m_caState = caState;
                this.PropertyName = propertyName;
            }

            public AnimatorStateMachine m_statemachine;
            public ChildAnimatorState m_caState;
            public string PropertyName { get; private set; }

            public float X
            {
                get
                {
                    if (null != this.m_statemachine)
                    {
                        switch (this.PropertyName)
                        {
                            case "anyStatePosition": return this.m_statemachine.anyStatePosition.x;
                            case "entryPosition": return this.m_statemachine.entryPosition.x;
                            case "exitPosition": return this.m_statemachine.exitPosition.x;
                            case "parentStateMachinePosition": return this.m_statemachine.parentStateMachinePosition.x;
                            default: throw new UnityException("未対応のプロパティー名だぜ☆（＾～＾）=ステートマシン.[" + this.PropertyName + "]");
                        }
                    }
                    else { return this.m_caState.position.x; }
                }
                set
                {
                    if (null != this.m_statemachine)
                    {
                        switch (this.PropertyName)
                        {
                            case "anyStatePosition": this.m_statemachine.anyStatePosition = new Vector3(value, this.m_statemachine.anyStatePosition.y, this.m_statemachine.anyStatePosition.z); break;
                            case "entryPosition": this.m_statemachine.entryPosition = new Vector3(value, this.m_statemachine.entryPosition.y, this.m_statemachine.entryPosition.z); break;
                            case "exitPosition": this.m_statemachine.exitPosition = new Vector3(value, this.m_statemachine.exitPosition.y, this.m_statemachine.exitPosition.z); break;
                            case "parentStateMachinePosition": this.m_statemachine.parentStateMachinePosition = new Vector3(value, this.m_statemachine.parentStateMachinePosition.y, this.m_statemachine.parentStateMachinePosition.z); break;
                            default: throw new UnityException("未対応のプロパティー名だぜ☆（＾～＾）=ステートマシン.[" + this.PropertyName + "]");
                        }
                    }
                    else { this.m_caState.position = new Vector3(value, this.m_caState.position.y, this.m_caState.position.z); }
                }
            }

            public float Y
            {
                get
                {
                    if (null != this.m_statemachine)
                    {
                        switch (this.PropertyName)
                        {
                            case "anyStatePosition": return this.m_statemachine.anyStatePosition.y;
                            case "entryPosition": return this.m_statemachine.entryPosition.y;
                            case "exitPosition": return this.m_statemachine.exitPosition.y;
                            case "parentStateMachinePosition": return this.m_statemachine.parentStateMachinePosition.y;
                            default: throw new UnityException("未対応のプロパティー名だぜ☆（＾～＾）=ステートマシン.[" + this.PropertyName + "]");
                        }
                    }
                    else { return this.m_caState.position.y; }
                }
                set
                {
                    if (null != this.m_statemachine)
                    {
                        switch (this.PropertyName)
                        {
                            case "anyStatePosition": this.m_statemachine.anyStatePosition = new Vector3( this.m_statemachine.anyStatePosition.x, value, this.m_statemachine.anyStatePosition.z); break;
                            case "entryPosition": this.m_statemachine.entryPosition = new Vector3( this.m_statemachine.entryPosition.x, value, this.m_statemachine.entryPosition.z); break;
                            case "exitPosition": this.m_statemachine.exitPosition = new Vector3( this.m_statemachine.exitPosition.x, value, this.m_statemachine.exitPosition.z); break;
                            case "parentStateMachinePosition": this.m_statemachine.parentStateMachinePosition = new Vector3( this.m_statemachine.parentStateMachinePosition.x, value, this.m_statemachine.parentStateMachinePosition.z); break;
                            default: throw new UnityException("未対応のプロパティー名だぜ☆（＾～＾）=ステートマシン.[" + this.PropertyName + "]");
                        }
                    }
                    else { this.m_caState.position = new Vector3( this.m_caState.position.x, value, this.m_caState.position.z); }
                }
            }

            public float Z
            {
                get
                {
                    if (null != this.m_statemachine)
                    {
                        switch (this.PropertyName)
                        {
                            case "anyStatePosition": return this.m_statemachine.anyStatePosition.z;
                            case "entryPosition": return this.m_statemachine.entryPosition.z;
                            case "exitPosition": return this.m_statemachine.exitPosition.z;
                            case "parentStateMachinePosition": return this.m_statemachine.parentStateMachinePosition.z;
                            default: throw new UnityException("未対応のプロパティー名だぜ☆（＾～＾）=ステートマシン.[" + this.PropertyName + "]");
                        }
                    }
                    else { return this.m_caState.position.z; }
                }
                set
                {
                    if (null != this.m_statemachine)
                    {
                        switch (this.PropertyName)
                        {
                            case "anyStatePosition": this.m_statemachine.anyStatePosition = new Vector3(this.m_statemachine.anyStatePosition.x, this.m_statemachine.anyStatePosition.y, value); break;
                            case "entryPosition": this.m_statemachine.entryPosition = new Vector3(this.m_statemachine.entryPosition.x, this.m_statemachine.entryPosition.y, value); break;
                            case "exitPosition": this.m_statemachine.exitPosition = new Vector3(this.m_statemachine.exitPosition.x, this.m_statemachine.exitPosition.y, value); break;
                            case "parentStateMachinePosition": this.m_statemachine.parentStateMachinePosition = new Vector3(this.m_statemachine.parentStateMachinePosition.x, this.m_statemachine.parentStateMachinePosition.y, value); break;
                            default: throw new UnityException("未対応のプロパティー名だぜ☆（＾～＾）=ステートマシン.[" + this.PropertyName + "]");
                        }
                    }
                    else { this.m_caState.position = new Vector3(this.m_caState.position.x, this.m_caState.position.y, value); }
                }
            }
        }

        static PositionRecord()
        {
            List<RecordDefinition> temp = new List<RecordDefinition>()
            {
                new RecordDefinition("#layerNum#"           ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("#machineStateNum#"    ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("#stateNum#"           ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("#transitionNum#"      ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("#conditionNum#"       ,RecordDefinition.FieldType.Int     ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("#proertyName#"        ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("#layerName#"          ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable          ,false),
                new RecordDefinition("#statemachinePath#"   ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable          ,false),
                new RecordDefinition("#stateName#"          ,RecordDefinition.FieldType.String  ,RecordDefinition.KeyType.Identifiable          ,false),
                new RecordDefinition("magnitude"            ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.None                  ,false),// リード・オンリー型
                new RecordDefinition("#normalized#"         ,RecordDefinition.FieldType.Other   ,RecordDefinition.KeyType.None                  ,false),
                new RecordDefinition("#normalizedX#"        ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.None                  ,false),
                new RecordDefinition("#normalizedY#"        ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.None                  ,false),
                new RecordDefinition("#normalizedZ#"        ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.None                  ,false),
                new RecordDefinition("sqrMagnitude"         ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.None                  ,false), // リード・オンリー型
                new RecordDefinition("x"                    ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.None,(object i)=>{ return ((PositionWrapper)i).X; }             ,(object i,float v)=>{ ((PositionWrapper)i).X = v; }),
                new RecordDefinition("y"                    ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.None,(object i)=>{ return ((PositionWrapper)i).Y; }             ,(object i,float v)=>{ ((PositionWrapper)i).Y = v; }),
                new RecordDefinition("z"                    ,RecordDefinition.FieldType.Float   ,RecordDefinition.KeyType.None,(object i)=>{ return ((PositionWrapper)i).Z; }             ,(object i,float v)=>{ ((PositionWrapper)i).Z = v; }),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }

            Empty = new PositionRecord(-1,-1,-1,-1,-1,"",new Vector3());
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static PositionRecord Empty { get; private set; }

        public PositionRecord(int layerNum, int machineStateNum, int stateNum, int transitionNum, int conditionNum, string proertyName, Vector3 position)
        {
            this.Fields = new Dictionary<string, object>()
            {
                { "#layerNum#"          ,layerNum           },
                { "#machineStateNum#"   ,machineStateNum    },
                { "#stateNum#"          ,stateNum           },
                { "#transitionNum#"     ,transitionNum      },
                { "#conditionNum#"      ,conditionNum       },
                { "#proertyName#"       ,proertyName        },
                { "#layerName#"         ,""                                            },
                { "#statemachinePath#"  ,""                                            },
                { "#stateName#"         ,""                                            },
                { "magnitude"           ,position.magnitude },
                { "#normalized#"        ,position.normalized == null ? "" : position.normalized.ToString()      },
                { "#normalizedX#"       ,position.normalized == null ? "" : position.normalized.x.ToString()    },
                { "#normalizedY#"       ,position.normalized == null ? "" : position.normalized.y.ToString()    },
                { "#normalizedZ#"       ,position.normalized == null ? "" : position.normalized.z.ToString()    },
                { "sqrMagnitude"        ,position.sqrMagnitude  },
                { "x"                   ,position.x     },
                { "y"                   ,position.y     },
                { "z"                   ,position.z     },
            };
        }
        public Dictionary<string, object> Fields { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">contents (コンテンツ)</param>
        /// <param name="n">output column name (列名出力)</param>
        /// <param name="d">output definition (列定義出力)</param>
        public void AppendCsvLine(StringBuilder c, bool n, bool d)
        {
            Definitions["#layerNum#"        ].AppendCsv(Fields, c, n, d);
            Definitions["#machineStateNum#" ].AppendCsv(Fields, c, n, d);
            Definitions["#stateNum#"        ].AppendCsv(Fields, c, n, d);
            Definitions["#transitionNum#"   ].AppendCsv(Fields, c, n, d);
            Definitions["#conditionNum#"    ].AppendCsv(Fields, c, n, d);
            Definitions["#proertyName#"     ].AppendCsv(Fields, c, n, d);
            Definitions["#layerName#"       ].AppendCsv(Fields, c, n, d);
            Definitions["#statemachinePath#"].AppendCsv(Fields, c, n, d);
            Definitions["#stateName#"       ].AppendCsv(Fields, c, n, d);
            Definitions["magnitude"         ].AppendCsv(Fields, c, n, d);
            Definitions["#normalized#"      ].AppendCsv(Fields, c, n, d);
            Definitions["#normalizedX#"     ].AppendCsv(Fields, c, n, d);
            Definitions["#normalizedY#"     ].AppendCsv(Fields, c, n, d);
            Definitions["#normalizedZ#"     ].AppendCsv(Fields, c, n, d);
            Definitions["sqrMagnitude"      ].AppendCsv(Fields, c, n, d);
            Definitions["x"                 ].AppendCsv(Fields, c, n, d);
            Definitions["y"                 ].AppendCsv(Fields, c, n, d);
            Definitions["z"                 ].AppendCsv(Fields, c, n, d);
            if (n) { c.Append("[EOL],"); }
            if (!d) { c.AppendLine(); }
        }
    }

    public class AconData
    {
        public AconData()
        {
            table_parameter = new HashSet<ParameterRecord>();
            table_layer = new List<LayerRecord>();
            table_statemachine = new List<StatemachineRecord>();
            table_state = new HashSet<StateRecord>();
            table_transition = new HashSet<TransitionRecord>();
            table_condition = new List<ConditionRecord>();
            table_position = new List<PositionRecord>();
        }

        public HashSet<ParameterRecord> table_parameter { get; set; }
        public List<LayerRecord> table_layer { get; set; }
        public List<StatemachineRecord> table_statemachine { get; set; }
        public HashSet<StateRecord> table_state { get; set; }
        public HashSet<TransitionRecord> table_transition { get; set; }
        public List<ConditionRecord> table_condition { get; set; }
        public List<PositionRecord> table_position { get; set; }
    }


    public abstract class AconDataUtility
    {

        public static void WriteCsv_Parameters(AconData aconData, string aconName, bool outputDefinition, StringBuilder message)
        {
            StringBuilder contents = new StringBuilder();
            if (outputDefinition)
            {
                RecordDefinition.AppendDefinitionHeader(contents); // 列定義ヘッダー出力
                ParameterRecord.Empty.AppendCsvLine(contents, false, outputDefinition); // 列定義出力
            }
            else
            {
                ParameterRecord.Empty.AppendCsvLine(contents, true, outputDefinition); // 列名出力
                foreach (ParameterRecord record in aconData.table_parameter) { record.AppendCsvLine(contents, false, outputDefinition); }
            }

            contents.AppendLine("[EOF],");
            StellaQLWriter.Write(StellaQLWriter.Filepath_LogParameters(aconName, outputDefinition), contents, message);
        }

        public static void WriteCsv_Layers(AconData aconData, string aconName, bool outputDefinition, StringBuilder message)
        {
            StringBuilder contents = new StringBuilder();

            if (outputDefinition) {
                RecordDefinition.AppendDefinitionHeader(contents); // 列定義ヘッダー出力
                LayerRecord.Empty.AppendCsvLine(contents, false, outputDefinition); // 列定義出力
            }
            else
            {
                LayerRecord.Empty.AppendCsvLine(contents, true, outputDefinition); // 列名出力
                foreach (LayerRecord record in aconData.table_layer) { record.AppendCsvLine(contents, false, outputDefinition); }
            }

            contents.AppendLine("[EOF],");
            StellaQLWriter.Write(StellaQLWriter.Filepath_LogLayer(aconName, outputDefinition), contents, message);
        }

        public static void WriteCsv_Statemachines(AconData aconData, string aconName, bool outputDefinition, StringBuilder message)
        {
            StringBuilder contents = new StringBuilder();

            if (outputDefinition)
            {
                RecordDefinition.AppendDefinitionHeader(contents); // 列定義ヘッダー出力
                StatemachineRecord.Empty.AppendCsvLine(contents, false, outputDefinition); // 列定義出力
            }
            else {
                StatemachineRecord.Empty.AppendCsvLine(contents, true, outputDefinition); // 列名出力
                foreach (StatemachineRecord record in aconData.table_statemachine) { record.AppendCsvLine(contents, false, outputDefinition); }
            }

            contents.AppendLine("[EOF],");
            StellaQLWriter.Write(StellaQLWriter.Filepath_LogStatemachine(aconName, outputDefinition), contents, message);
        }

        public static void CreateCsvTable_State( HashSet<StateRecord> table, bool outputDefinition, StringBuilder contents)
        {
            if (outputDefinition)
            {
                RecordDefinition.AppendDefinitionHeader(contents); // 列定義ヘッダー出力
                StateRecord.Empty.AppendCsvLine(contents, false, outputDefinition); // 列定義出力
            }
            else {
                StateRecord.Empty.AppendCsvLine(contents, true, outputDefinition); // 列名出力
                foreach (StateRecord stateRecord in table) { stateRecord.AppendCsvLine(contents, false, outputDefinition); }
            }
        }
        public static void WriteCsv_States( AconData aconData, string aconName, bool outputDefinition, StringBuilder message)
        {
            StringBuilder contents = new StringBuilder();
            CreateCsvTable_State( aconData.table_state, outputDefinition, contents);

            contents.AppendLine("[EOF],");
            StellaQLWriter.Write(StellaQLWriter.Filepath_LogStates(aconName, outputDefinition), contents, message);
        }

        public static void CreateCsvTable_Transition(HashSet<TransitionRecord> table, bool outputDefinition, StringBuilder contents)
        {
            if (outputDefinition) // 列定義シートを作る場合
            {
                RecordDefinition.AppendDefinitionHeader(contents); // 列定義ヘッダー出力
                TransitionRecord.Empty.AppendCsvLine(contents, false, outputDefinition); // 列定義出力
            }
            else {
                TransitionRecord.Empty.AppendCsvLine(contents, true, outputDefinition); // 列名出力
                foreach (TransitionRecord record in table) { record.AppendCsvLine(contents, false, outputDefinition); }
            }
        }
        public static void WriteCsv_Transitions(AconData aconData, string aconName, bool outputDefinition, StringBuilder message)
        {
            StringBuilder contents = new StringBuilder();
            CreateCsvTable_Transition(aconData.table_transition, outputDefinition, contents);

            contents.AppendLine("[EOF],");
            StellaQLWriter.Write(StellaQLWriter.Filepath_LogTransition(aconName, outputDefinition), contents, message);
        }

        public static void WriteCsv_Conditions(AconData aconData, string aconName, bool outputDefinition, StringBuilder message)
        {
            StringBuilder contents = new StringBuilder();

            if (outputDefinition)
            {
                RecordDefinition.AppendDefinitionHeader(contents); // 列定義ヘッダー出力
                ConditionRecord.Empty.AppendCsvLine(contents, false, outputDefinition); // 列定義出力
            }
            else {
                ConditionRecord.Empty.AppendCsvLine(contents, true, outputDefinition); // 列名出力
                foreach (ConditionRecord record in aconData.table_condition) { record.AppendCsvLine(contents, false, outputDefinition); }
            }

            contents.AppendLine("[EOF],");
            StellaQLWriter.Write(StellaQLWriter.Filepath_LogConditions(aconName, outputDefinition), contents, message);
        }

        public static void WriteCsv_Positions(AconData aconData, string aconName, bool outputDefinition, StringBuilder message)
        {
            StringBuilder contents = new StringBuilder();

            if (outputDefinition)
            {
                RecordDefinition.AppendDefinitionHeader(contents); // 列定義ヘッダー出力
                PositionRecord.Empty.AppendCsvLine(contents, false, outputDefinition); // 列定義出力
            }
            else {
                PositionRecord.Empty.AppendCsvLine(contents, true, outputDefinition); // 列名出力
                foreach (PositionRecord record in aconData.table_position) { record.AppendCsvLine(contents, false, outputDefinition); }
            }

            contents.AppendLine("[EOF],");
            StellaQLWriter.Write(StellaQLWriter.Filepath_LogPositions(aconName, outputDefinition), contents, message);
        }
    }
}
