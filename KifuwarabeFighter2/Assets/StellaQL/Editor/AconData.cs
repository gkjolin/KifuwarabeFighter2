﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DojinCircleGrayscale.StellaQL
{
    public class AnimatorControllerWrapper
    {
        public AnimatorControllerWrapper(AnimatorController ac)
        {
            SourceAc = ac;

            CopiedLayers = new List<AnimatorControllerLayer>();

            // レイヤーのセッターは機能していない？　コピーを作って使うことにする。
            foreach (AnimatorControllerLayer actualLayer in ac.layers)
            {
                AnimatorControllerLayer copiedLayer = AconDeepcopy.DeepcopyLayer(actualLayer);
                CopiedLayers.Add(copiedLayer);
            }
        }

        public AnimatorController SourceAc { get; private set; }
        public List<AnimatorControllerLayer> CopiedLayers { get; private set; }
    }

    public class RecordDefinition
    {
        public enum FieldType
        {
            Int,
            Float,
            Bool,
            String,
            /// <summary>
            /// スプレッドシートのマクロで実装がハードコーディングされている文字列。
            /// </summary>
            SpecialString,
            /// <summary>
            /// 対応していないフィールド。
            /// </summary>
            Other,
        }

        public enum SubFieldType
        {
            /// <summary>
            /// TODO: スプレッドシートで、文字列型のClear欄を非表示にする。用意はしてみたものの、使う場面がない……。
            /// </summary>
            Required,
            None,
        }

        public enum KeyType
        {
            /// <summary>
            /// StellaQLスプレッドシートで使う、一時的なナンバリング
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
            /// <summary>
            /// StellaQL側で書き込みに対応していない場合はこれ。
            /// </summary>
            StellaQLSpreadsheetDoesNotSupportWriting,
            /// <summary>
            /// それ以外
            /// </summary>
            None,
        }

        public RecordDefinition(string name, FieldType type, SubFieldType subType, KeyType keyField, bool input)
        {
            this.Name = name;
            this.Type = type;
            this.SubType = subType;
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
        public RecordDefinition(string name, FieldType type, SubFieldType subType, KeyType keyField, GettterBool getter, SetterBool setter)       : this(name, type, subType, keyField, true)
        {
            this.m_getterBool = getter;     this.m_setterBool = setter;
        }
        public RecordDefinition(string name, FieldType type, SubFieldType subType, KeyType keyField, GettterFloat getter, SetterFloat setter)     : this(name,type, subType, keyField, true)
        {
            this.m_getterFloat = getter;    this.m_setterFloat = setter;
        }
        public RecordDefinition(string name, FieldType type, SubFieldType subType, KeyType keyField, GettterInt getter, SetterInt setter)         : this(name, type, subType, keyField, true)
        {
            this.m_getterInt = getter;      this.m_setterInt = setter;
        }
        public RecordDefinition(string name, FieldType type, SubFieldType subType, KeyType keyField, GettterString getter, SetterString setter)   : this(name, type, subType, keyField, true)
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
        public SubFieldType SubType { get; private set; }

        /// <summary>
        /// 列の種類
        /// </summary>
        public KeyType KeyField { get; private set; }

        /// <summary>
        /// スプレッド・シートから入力可能か
        /// </summary>
        public bool Input { get; private set; }

        /// <summary>
        /// 列の記入漏れを防ぐために　ひとくくりにしています。
        /// </summary>
        public void AppendCsv(Dictionary<string, object> fields, StringBuilder contents, bool outputColumnName, bool outputDefinition)
        {
            // 列定義一列分を出力するなら
            if (outputDefinition)
            {
                // 列名を出力するなら
                if (outputColumnName)
                {
                }
                else
                {
                    contents.Append(Name); contents.Append(",");

                    // １文字目。列挙型の要素名の先頭を小文字にして、型名とする。
                    contents.Append(Type.ToString().Substring(0, 1).ToLower());

                    // ２文字目以降。
                    contents.Append(Type.ToString().Substring(1));
                    contents.Append(",");

                    contents.Append(KeyField); contents.Append(",");
                    contents.Append(Input); contents.Append(",");
                    contents.Append(SubType); contents.Append(",");             // 2017-02-14 追加
                    contents.AppendLine();
                }
            }
            // 1フィールド分を出力するなら
            else
            {
                // 列名を出力するなら
                if (outputColumnName)
                {
                    switch (Type)
                    {
                        case FieldType.Int://thru
                        case FieldType.Float:
                        case FieldType.Bool: contents.Append(Name); contents.Append(","); break;
                        case FieldType.String://thru
                        case FieldType.Other:
                        default: contents.Append(Name); contents.Append(","); break;
                    }
                }
                else
                {
                    switch (Type)
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
            // 列定義ヘッダー出力
            contents.AppendLine("Name,Type,KeyField,Input,SubType,[EOL],");
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
                case FieldType.Bool:    if ((bool)actualOld != (bool)requestOld) { throw new UnityException("Old value is different. (bool type) actual=[" + actualOld + "] old=[" + requestOld + "]"); } break;
                case FieldType.Float:   if ((float)actualOld != (float)requestOld) { throw new UnityException("Old value is different. (float type) actual=[" + actualOld + "] old=[" + requestOld + "]"); } break;
                case FieldType.Int:     if ((int)actualOld != (int)requestOld) { throw new UnityException("Old value is different. (int type) actual=[" + actualOld + "] old=[" + requestOld + "]"); } break;
                case FieldType.Other:   throw new UnityException("Does not support Other type. FieldType=[" + Type.ToString() + "]");
                case FieldType.String:  if ((string)actualOld != (string)requestOld) { throw new UnityException("Old value is different. (string type) actual=[" + actualOld + "] old=[" + requestOld + "]"); } break;
                default: throw new UnityException("Does not support. FieldType=[" + Type.ToString() + "]");
            }            
            return true;
        }

        /// <summary>
        /// 既存のオブジェクトのプロパティー更新の場合、これを使う。
        /// </summary>
        /// <param name="instance">ステートマシン、チャイルド・ステート、コンディション・ラッパー、ポジション・ラッパー等</param>
        public void Update(object instance, DataManipulationRecord record, StringBuilder message)
        {
            if (null == instance) { throw new UnityException("Instance is null."); }
            switch (Type) {
                case FieldType.Bool: {
                        if (null == m_getterBool) { throw new UnityException("m_getterBool is null."); }
                        bool actual = m_getterBool(instance);
                        if (EqualsOld(actual, record.OldBool)) { m_setterBool(instance, record.NewBool); }
                    } break;
                case FieldType.Float: {
                        if (null == m_getterFloat) { throw new UnityException("m_getterFloat is null."); }
                        float actual = m_getterFloat(instance);
                        if (EqualsOld(actual, record.OldFloat)) { m_setterFloat(instance, record.NewFloat); }
                    } break;
                case FieldType.Int: {
                        if (null == m_getterInt) { throw new UnityException("m_getterInt is null."); }
                        int actual = m_getterInt(instance);
                        if (EqualsOld(actual, record.OldInt)) { m_setterInt(instance, record.NewInt); }
                    } break;

                // 未対応は、この型にしてある
                case FieldType.Other: break;

                case FieldType.String: {
                        if (null == m_getterString) { throw new UnityException("m_getterString is null."); }
                        string actual = m_getterString(instance);
                        if (EqualsOld(actual, record.Old)) {
                            // 空文字列にセットする
                            if (record.IsClear) { m_setterString(instance, ""); }
                            else { m_setterString(instance, record.New); }
                        }
                    } break;
                default: throw new UnityException("Not supported. FieldType=["+Type.ToString()+"]");
            }
        }
    }

    public interface AconObjectRecordable
    {
        /// <summary>
        /// CSVを１行出力します。
        /// </summary>
        /// <param name="c">コンテンツ contents</param>
        /// <param name="n">列名出力 output column name</param>
        /// <param name="d">列定義出力 output definition</param>
        void AppendCsvLine(StringBuilder c, bool n, bool d);
    }

    /// <summary>
    /// パラメーター
    /// </summary>
    public class ParameterRecord : AconObjectRecordable
    {
        static ParameterRecord()
        {
            List<RecordDefinition> temp = new List<RecordDefinition>()
            {
                new RecordDefinition("num"          , RecordDefinition.FieldType.Int        ,RecordDefinition.SubFieldType.None    ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("#name_ID#"    , RecordDefinition.FieldType.String     ,RecordDefinition.SubFieldType.None    ,RecordDefinition.KeyType.Identifiable          ,false),
                new RecordDefinition("name"         , RecordDefinition.FieldType.String     ,RecordDefinition.SubFieldType.None    ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{          return ((AnimatorControllerParameter)i).name; }
                    ,(object i,string v)=>{ ((AnimatorControllerParameter)i).name = v; }
                ),
                new RecordDefinition("#type_String#", RecordDefinition.FieldType.String     ,RecordDefinition.SubFieldType.None    ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{          return ((AnimatorControllerParameter)i).type.ToString(); }
                    ,(object i,string v)=>{ ((AnimatorControllerParameter)i).type = (AnimatorControllerParameterType)Enum.Parse(typeof(AnimatorControllerParameterType),v); }
                ),
                new RecordDefinition("defaultBool"  , RecordDefinition.FieldType.Bool       ,RecordDefinition.SubFieldType.None    ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{        return ((AnimatorControllerParameter)i).defaultBool; }
                    ,(object i,bool v)=>{ ((AnimatorControllerParameter)i).defaultBool = v; }
                ),
                new RecordDefinition("defaultFloat" , RecordDefinition.FieldType.Float      ,RecordDefinition.SubFieldType.None    ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{         return ((AnimatorControllerParameter)i).defaultFloat; }
                    ,(object i,float v)=>{ ((AnimatorControllerParameter)i).defaultFloat = v; }
                ),
                new RecordDefinition("defaultInt"   , RecordDefinition.FieldType.Int        ,RecordDefinition.SubFieldType.None    ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{       return ((AnimatorControllerParameter)i).defaultInt; }
                    ,(object i,int v)=>{ ((AnimatorControllerParameter)i).defaultInt = v; }
                ),
                new RecordDefinition("nameHash"     , RecordDefinition.FieldType.Int        ,RecordDefinition.SubFieldType.None    ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{       return ((AnimatorControllerParameter)i).nameHash; }
                    ,(object i,int v)=>{ throw new UnityException("Not supported.");}
                ),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }
            Empty = new ParameterRecord(-1, "", false, 0.0f, -1, 0, (AnimatorControllerParameterType)0);
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static AconObjectRecordable Empty { get; private set; }

        public ParameterRecord(int num, string name, bool numberBool, float numberFloat, int numberInt, int nameHash, AnimatorControllerParameterType type)
        {
            this.Fields = new Dictionary<string, object>()
            {
                { "num"             ,num                },
                { "#name_ID#"       ,name               }, // ID用 for key.
                { "name"            ,name               }, // 編集用 for edit.
                { "#type_String#"   ,type.ToString()    },
                { "defaultBool"     ,numberBool         },
                { "defaultFloat"    ,numberFloat        },
                { "defaultInt"      ,numberInt          },
                { "nameHash"        ,nameHash           },
            };
        }
        public Dictionary<string, object> Fields { get; set; }

        /// <param name="c">コンテンツ contents</param>
        /// <param name="n">列名出力 output column name</param>
        /// <param name="d">列定義出力 output definition</param>
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
    public class LayerRecord : AconObjectRecordable
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
                new RecordDefinition("#layerNum#"               ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("name"                     ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable          ,false),
                new RecordDefinition("#avatarMask_assetPath#"   ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{
                        if(null==((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].avatarMask) { return ""; }
                        return AssetDatabase.GetAssetPath(((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].avatarMask.GetInstanceID());
                    }
                    ,(object i,string v)=>{
                        AvatarMask value = AssetDatabase.LoadAssetAtPath<AvatarMask>(v);
                        if(null==value) { throw new UnityException("Not found ["+v+"] avatar mask."); }
                        ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].avatarMask = value;
                        // Operation_Layer.DumpLog(((LayerWrapper)i).SourceAcWrapper);
                        // TODO: Delete にも対応したい。
                    }),
                new RecordDefinition("#blendingMode_string#"    ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{ return ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].blendingMode.ToString(); }
                    ,(object i,string v)=>{
                        HashSet<AnimatorLayerBlendingMode> hits = Operation_AnimatorLayerBlendingMode.FetchAnimatorLayerBlendingModes(v);
                        if(0==hits.Count) { throw new UnityException("Not found. v=["+v+"] hits.Count=["+hits.Count+"]"); }
                        else if(1<hits.Count)
                        {
                            StringBuilder sb = new StringBuilder(); foreach(AnimatorLayerBlendingMode enumItem in hits) { sb.Append(enumItem.ToString()); sb.Append(" "); }
                            throw new UnityException("There was more than one. v=["+v+"] hits.Count=["+hits.Count+"] sb="+sb.ToString());
                        }
                        AnimatorLayerBlendingMode value = 0; bool found =false;
                        foreach(AnimatorLayerBlendingMode enumItem in hits) { value = enumItem; found=true; break; }
                        if(found) {
                            ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].blendingMode = value;
                            //Operation_Layer.DumpLog(((LayerWrapper)i).SourceAcWrapper);
                        }
                    }),
                new RecordDefinition("defaultWeight"            ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{ return ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].defaultWeight;           }
                    ,(object i,float v)=>{
                        ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].defaultWeight = v;
                        //Operation_Layer.DumpLog(((LayerWrapper)i).SourceAcWrapper);
                    }),
                new RecordDefinition("iKPass"                   ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None    ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{ return ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].iKPass;                  }
                    ,(object i,bool v)=>{
                        ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].iKPass = v;
                        //Operation_Layer.DumpLog(((LayerWrapper)i).SourceAcWrapper);
                    }),
                new RecordDefinition("syncedLayerAffectsTiming" ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None    ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{ return ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].syncedLayerAffectsTiming;}
                    ,(object i,bool v)=>{
                        ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].syncedLayerAffectsTiming = v;
                        //Operation_Layer.DumpLog(((LayerWrapper)i).SourceAcWrapper);
                    }),
                new RecordDefinition("syncedLayerIndex"         ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{ return ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].syncedLayerIndex;        }
                    ,(object i,int v)=>{
                        ((LayerWrapper)i).SourceAcWrapper.SourceAc.layers[((LayerWrapper)i).LayerIndex].syncedLayerIndex = v;
                        //Operation_Layer.DumpLog(((LayerWrapper)i).SourceAcWrapper);
                    }),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }
            Empty = new LayerRecord(-1, new AnimatorControllerLayer());
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static AconObjectRecordable Empty { get; private set; }

        public LayerRecord(int num, AnimatorControllerLayer layer)
        {
            this.Fields = new Dictionary<string, object>()
            {
                { "#layerNum#"                  ,num },
                { "name"                        ,layer.name},
                { "#avatarMask_assetPath#"      ,layer.avatarMask == null ? "" : AssetDatabase.GetAssetPath(layer.avatarMask.GetInstanceID()) },
                { "#blendingMode_string#"       ,layer.blendingMode.ToString()},
                { "defaultWeight"               ,layer.defaultWeight},
                { "iKPass"                      ,layer.iKPass},
                { "syncedLayerAffectsTiming"    ,layer.syncedLayerAffectsTiming},
                { "syncedLayerIndex"            ,layer.syncedLayerIndex},
            };
        }
        public Dictionary<string,object> Fields { get; set; }

        /// <param name="c">コンテンツ contents</param>
        /// <param name="n">列名出力 output column name</param>
        /// <param name="d">列定義出力 output definition</param>
        public void AppendCsvLine(StringBuilder c, bool n, bool d)
        {
            Definitions["#layerNum#"                ].AppendCsv(Fields, c, n, d);
            Definitions["name"                      ].AppendCsv(Fields, c, n, d);
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
    public class StatemachineRecord : AconObjectRecordable
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
                new RecordDefinition("#layerNum#"                   ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#machineStateNum#"            ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#layerName#"                  ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable      ,false), // Empty.
                new RecordDefinition("#statemachinePath#"           ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable      // If "Base Layer.Alpaca.Bear.Cat.Dog", It is "Alpaca.Bear.Cat".
                    ,(object i)=>{          return ((Wrapper)i).StatemachinePath; }
                    ,(object i,string v)=>{ throw new UnityException("Not supported.");}
                ),
                new RecordDefinition("name"                         ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.StellaQLSpreadsheetDoesNotSupportWriting
                    ,(object i)=>{          return ((Wrapper)i).Source.name; }
                    ,(object i,string v)=>{ throw new UnityException("Not supported.");}
                ),
                new RecordDefinition("#anyStateTransitions_Length#" ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.StellaQLSpreadsheetDoesNotSupportWriting
                    ,(object i)=>{          return ((Wrapper)i).Source.anyStateTransitions.Length.ToString(); }
                    ,(object i,string v)=>{ throw new UnityException("Not supported.");}
                ),
                new RecordDefinition("#behaviours_Length#"          ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.StellaQLSpreadsheetDoesNotSupportWriting
                    ,(object i)=>{          return ((Wrapper)i).Source.behaviours.Length.ToString(); }
                    ,(object i,string v)=>{ throw new UnityException("Not supported.");}
                ),
                new RecordDefinition("#defaultState_String#"        ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.StellaQLSpreadsheetDoesNotSupportWriting
                    ,(object i)=>{          return (((Wrapper)i).Source.defaultState==null) ? "" : ((Wrapper)i).Source.defaultState.ToString(); }
                    ,(object i,string v)=>{ throw new UnityException("Not supported.");}
                ),
                new RecordDefinition("#entryTransitions_Length#"    ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.StellaQLSpreadsheetDoesNotSupportWriting
                    ,(object i)=>{          return ((Wrapper)i).Source.entryTransitions.Length.ToString(); }
                    ,(object i,string v)=>{ throw new UnityException("Not supported.");}
                ),
                new RecordDefinition("hideFlags"                    ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.StellaQLSpreadsheetDoesNotSupportWriting
                    ,(object i)=>{          return ((Wrapper)i).Source.hideFlags.ToString(); }
                    ,(object i,string v)=>{ ((Wrapper)i).Source.hideFlags = (HideFlags)System.Enum.Parse(typeof(HideFlags), v);}
                ),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }

            Empty = new StatemachineRecord(-1,-1,"",new AnimatorStateMachine(),new HashSet<PositionRecord>());
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static AconObjectRecordable Empty { get; private set; }

        public StatemachineRecord(int layerNum, int machineStateNum, string statemachinePath, AnimatorStateMachine stateMachine, HashSet<PositionRecord> positionsTable)
        {
            this.Fields = new Dictionary<string, object>()
            {
                { "#layerNum#"                      ,layerNum                                                                                           },
                { "#machineStateNum#"               ,machineStateNum                                                                                    },
                { "#layerName#"                     ,""                                                                                                 },// Empty
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

        /// <param name="c">コンテンツ contents</param>
        /// <param name="n">列名出力 output column name</param>
        /// <param name="d">列定義出力 output definition</param>
        public void AppendCsvLine(StringBuilder c, bool n, bool d)
        {
            Definitions["#layerNum#"                    ].AppendCsv(Fields, c, n, d);
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
    public class StateRecord : AconObjectRecordable
    {
        public class Wrapper
        {
            public Wrapper(AnimatorState source)
            {
                Source = source;
            }

            public AnimatorState Source { get; private set; }
        }

        static StateRecord()
        {
            List<RecordDefinition> temp = new List<RecordDefinition>()
            {
                new RecordDefinition("#layerNum#"                   ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#machineStateNum#"            ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#stateNum#"                   ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#layerName#"                  ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable      ,false), // Empty.
                new RecordDefinition("#statemachinePath#"           ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable      ,false), // Empty.
                new RecordDefinition("name"                         ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable  ,false) ,
                new RecordDefinition("cycleOffset"                  ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.cycleOffset; }         ,(object i,float v)=>{ ((Wrapper)i).Source.cycleOffset = v; }),
                new RecordDefinition("cycleOffsetParameter"         ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.cycleOffsetParameter; },(object i,string v)=>{ ((Wrapper)i).Source.cycleOffsetParameter = v; }),
                new RecordDefinition("#hideFlags_string#"           ,RecordDefinition.FieldType.Other   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None          ,false),
                new RecordDefinition("iKOnFeet"                     ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.iKOnFeet; }            ,(object i,bool v)=>{ ((Wrapper)i).Source.iKOnFeet = v; }),
                new RecordDefinition("mirror"                       ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.mirror; }              ,(object i,bool v)=>{ ((Wrapper)i).Source.mirror = v; }),
                new RecordDefinition("mirrorParameter"              ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.mirrorParameter; }     ,(object i,string v)=>{ ((Wrapper)i).Source.mirrorParameter = v; }),
                new RecordDefinition("mirrorParameterActive"        ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.mirrorParameterActive;},(object i,bool v)=>{ ((Wrapper)i).Source.mirrorParameterActive = v; }),
                new RecordDefinition("#motion_assetPath#"           ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None          ,false),
                new RecordDefinition("nameHash"                     ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None          ,false),
                new RecordDefinition("speed"                        ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.speed; }               ,(object i,float v)=>{ ((Wrapper)i).Source.speed = v; }),
                new RecordDefinition("speedParameter"               ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.speedParameter; }      ,(object i,string v)=>{ ((Wrapper)i).Source.speedParameter = v; }),
                new RecordDefinition("speedParameterActive"         ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.speedParameterActive; },(object i,bool v)=>{ ((Wrapper)i).Source.speedParameterActive = v; }),
                new RecordDefinition("tag"                          ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.tag; }                 ,(object i,string v)=>{ ((Wrapper)i).Source.tag = v; }),
                new RecordDefinition("writeDefaultValues"           ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,(object i)=>{ return ((Wrapper)i).Source.writeDefaultValues; }  ,(object i,bool v)=>{ ((Wrapper)i).Source.writeDefaultValues = v; }),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }

            Empty = new StateRecord(-1,-1,-1,new AnimatorState());
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static AconObjectRecordable Empty { get; private set; }

        public static StateRecord CreateInstance(int layerNum, int machineStateNum, int stateNum, string parentPath, ChildAnimatorState caState, HashSet<PositionRecord> positionsTable)
        {
            positionsTable.Add(new PositionRecord(layerNum, machineStateNum, stateNum, -1, -1, "position", caState.position));
            return new StateRecord(layerNum, machineStateNum, stateNum, caState.state);
        }
        public StateRecord(int layerNum, int machineStateNum, int stateNum, AnimatorState state)
        {
            this.Fields = new Dictionary<string, object>()
            {
                { "#layerNum#"              ,layerNum                   },
                { "#machineStateNum#"       ,machineStateNum            },
                { "#stateNum#"              ,stateNum                   },
                { "#layerName#"             ,""                         }, // 空っぽ。スプレッドシート側で探索して入れる。
                { "#statemachinePath#"      ,""                         }, // 空っぽ。スプレッドシート側で探索して入れる。
                { "name"                    ,state.name                 },
                { "cycleOffset"             ,state.cycleOffset          },
                { "cycleOffsetParameter"    ,state.cycleOffsetParameter },
                { "#hideFlags_string#"      ,state.hideFlags.ToString() }, // 文字列化 toString
                { "iKOnFeet"                ,state.iKOnFeet             },
                { "mirror"                  ,state.mirror               },
                { "mirrorParameter"         ,state.mirrorParameter      },
                { "mirrorParameterActive"   ,state.mirrorParameterActive},

                 // name だと一意性がないのでアセット・パスを入れる。
                { "#motion_assetPath#"      ,state.motion == null ? "" : AssetDatabase.GetAssetPath(state.motion.GetInstanceID()) },

                { "nameHash"                ,state.nameHash             },
                { "speed"                   ,state.speed                },
                { "speedParameter"          ,state.speedParameter       },
                { "speedParameterActive"    ,state.speedParameterActive },
                { "tag"                     ,state.tag                  },
                { "writeDefaultValues"      ,state.writeDefaultValues   },
            };
        }
        public Dictionary<string, object> Fields { get; set; }

        /// <param name="c">コンテンツ contents</param>
        /// <param name="n">列名出力 output column name</param>
        /// <param name="d">列定義出力 output definition</param>
        public void AppendCsvLine(StringBuilder c, bool n, bool d)
        {
            Definitions["#layerNum#"            ].AppendCsv(Fields, c, n, d);
            Definitions["#machineStateNum#"     ].AppendCsv(Fields, c, n, d);
            Definitions["#stateNum#"            ].AppendCsv(Fields, c, n, d);
            Definitions["#layerName#"           ].AppendCsv(Fields, c, n, d);
            Definitions["#statemachinePath#"    ].AppendCsv(Fields, c, n, d);
            Definitions["name"                  ].AppendCsv(Fields, c, n, d);
            Definitions["cycleOffset"           ].AppendCsv(Fields, c, n, d);
            Definitions["cycleOffsetParameter"  ].AppendCsv(Fields, c, n, d);
            Definitions["#hideFlags_string#"    ].AppendCsv(Fields, c, n, d);
            Definitions["iKOnFeet"              ].AppendCsv(Fields, c, n, d);
            Definitions["mirror"                ].AppendCsv(Fields, c, n, d);
            Definitions["mirrorParameter"       ].AppendCsv(Fields, c, n, d);
            Definitions["mirrorParameterActive" ].AppendCsv(Fields, c, n, d);
            Definitions["#motion_assetPath#"    ].AppendCsv(Fields, c, n, d);
            Definitions["nameHash"              ].AppendCsv(Fields, c, n, d);
            Definitions["speed"                 ].AppendCsv(Fields, c, n, d);
            Definitions["speedParameter"        ].AppendCsv(Fields, c, n, d);
            Definitions["speedParameterActive"  ].AppendCsv(Fields, c, n, d);
            Definitions["tag"                   ].AppendCsv(Fields, c, n, d);
            Definitions["writeDefaultValues"    ].AppendCsv(Fields, c, n, d);
            if (n) { c.Append("[EOL],"); }
            if (!d) { c.AppendLine(); }
        }
    }

    /// <summary>
    /// トランジション
    /// 
    /// (コンディションは別)
    /// </summary>
    public class TransitionRecord : AconObjectRecordable
    {
        static TransitionRecord()
        {
            List<RecordDefinition> temp = new List<RecordDefinition>()
            {
                new RecordDefinition("#layerNum#"                       ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#machineStateNum#"                ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#stateNum#"                       ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#transitionNum#"                  ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering,false),
                new RecordDefinition("#layerName#"                      ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable      ,false),
                new RecordDefinition("#statemachinePath#"               ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable      ,false),
                new RecordDefinition("#stateName#"                      ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable      ,false),
                new RecordDefinition("name"                             ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable      ,false),
                new RecordDefinition("#stellaQLComment#"                ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,false),
                new RecordDefinition("canTransitionToSelf"              ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).canTransitionToSelf; }   ,(object i,bool v)=>{ ((AnimatorStateTransition)i).canTransitionToSelf = v; }),
                new RecordDefinition("#destinationState_name#"          ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,false),
                new RecordDefinition("#destinationState_nameHash#"      ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,false),
                new RecordDefinition("#destinationStateMachine_name#"   ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,false),
                new RecordDefinition("duration"                         ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).duration; }              ,(object i,float v)=>{ ((AnimatorStateTransition)i).duration = v; }),
                new RecordDefinition("exitTime"                         ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).exitTime; }              ,(object i,float v)=>{ ((AnimatorStateTransition)i).exitTime = v; }),
                new RecordDefinition("hasExitTime"                      ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).hasExitTime; }           ,(object i,bool v)=>{ ((AnimatorStateTransition)i).hasExitTime = v; }),
                new RecordDefinition("hasFixedDuration"                 ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).hasFixedDuration; }      ,(object i,bool v)=>{ ((AnimatorStateTransition)i).hasFixedDuration = v; }),
                new RecordDefinition("hideFlags"                        ,RecordDefinition.FieldType.Other   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,false),
                new RecordDefinition("interruptionSource"               ,RecordDefinition.FieldType.Other   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,false),
                new RecordDefinition("isExit"                           ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).isExit; }                ,(object i,bool v)=>{ ((AnimatorStateTransition)i).isExit = v; }),
                new RecordDefinition("mute"                             ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).mute; }                  ,(object i,bool v)=>{ ((AnimatorStateTransition)i).mute = v; }),
                new RecordDefinition("offset"                           ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).offset; }                ,(object i,float v)=>{ ((AnimatorStateTransition)i).offset = v; }),
                new RecordDefinition("orderedInterruption"              ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).orderedInterruption; }   ,(object i,bool v)=>{ ((AnimatorStateTransition)i).orderedInterruption = v; }),
                new RecordDefinition("solo"                             ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None              ,(object i)=>{ return ((AnimatorStateTransition)i).solo; }                  ,(object i,bool v)=>{ ((AnimatorStateTransition)i).solo = v; }),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }

            Empty = new TransitionRecord(-1,-1,-1,-1,new AnimatorStateTransition(),"");
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static AconObjectRecordable Empty { get; private set; }

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
                { "#stellaQLComment#"               , stellaQLComment                               }, // For debug.
                { "canTransitionToSelf"             , transition.canTransitionToSelf                },
                { "#destinationState_name#"         , transition.destinationState == null ? "" : transition.destinationState.name},
                { "#destinationState_nameHash#"     , transition.destinationState == null ? 0 : transition.destinationState.nameHash},
                { "#destinationStateMachine_name#"  , transition.destinationStateMachine == null ? "" : transition.destinationStateMachine.name},
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

        /// <param name="c">コンテンツ contents</param>
        /// <param name="n">列名出力 output column name</param>
        /// <param name="d">列定義出力 output definition</param>
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
    /// UnityEditor の AnimatorCondition のセッターが機能していないと推測を立て、別途用意。
    /// </summary>
    public class AconConditionBuilder
    {
        public AconConditionBuilder(AnimatorCondition old)
        {
            mode = old.mode;
            threshold = old.threshold;
            parameter = old.parameter;
        }
        public AnimatorConditionMode mode { get; set; }
        public float threshold { get; set; }
        public string parameter { get; set; }
    }

    /// <summary>
    /// コンディション
    /// </summary>
    public class ConditionRecord : AconObjectRecordable
    {
        /// <summary>
        /// struct を object に渡したいときに使うラッパー
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

            /// <param name="conditionNum">データ識別のために。</param>
            /// <param name="sourceParentTransition">オブジェクトの再描画のために。</param>
            /// <param name="source"></param>
            public AnimatorConditionWrapper(int conditionNum, AnimatorStateTransition sourceParentTransition, AnimatorCondition source)
            {
                ConditionNum = conditionNum;
                m_sourceParentTransition = sourceParentTransition;
                this.m_source = source;
                this.IsNull = false;
            }

            public bool IsNull { get; private set; }
            public int ConditionNum { get; private set; }
            /// <summary>
            /// コンディションへの更新を反映するためには、親トランジションの AddCondition( ) メソッドが必要なようだ。
            /// </summary>
            public AnimatorStateTransition m_sourceParentTransition;
            public AnimatorCondition m_source;
        }

        /// <summary>
        /// モードには、数値型のときは演算子が入っているし、論理値型のときは論理値が入っている。
        /// </summary>
        public static string Mode_to_string(AnimatorConditionMode mode)
        {
            if (0 == mode) { return ""; }

            switch (mode)
            {
                case AnimatorConditionMode.Greater: return ">";
                case AnimatorConditionMode.Less: return "<";
                case AnimatorConditionMode.Equals: return "=";
                case AnimatorConditionMode.NotEqual: return "<>";
                case AnimatorConditionMode.If: return "TRUE";
                case AnimatorConditionMode.IfNot: return "FALSE";
                default: throw new UnityException("Not supported. mode = [" + mode + "]");
            }
        }
        /// <summary>
        /// モードには、数値型のときは演算子が入っているし、論理値型のときは論理値が入っている。
        /// </summary>
        public static AnimatorConditionMode String_to_mode(string modeString)
        {
            if ("" == modeString) { return 0; }

            switch (modeString.Trim().ToUpper())
            {
                case ">": return AnimatorConditionMode.Greater;
                case "<": return AnimatorConditionMode.Less;
                case "=": return AnimatorConditionMode.Equals;
                case "<>": return AnimatorConditionMode.NotEqual;
                case "TRUE": return AnimatorConditionMode.If;
                case "FALSE": return AnimatorConditionMode.IfNot;
                default: throw new UnityException("Not supported. mode string = [" + modeString + "]");
            }
        }

        static ConditionRecord()
        {
            List<RecordDefinition> temp = new List<RecordDefinition>()
            {
                new RecordDefinition("#layerNum#"           ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering    ,false  ),
                new RecordDefinition("#machineStateNum#"    ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering    ,false  ),
                new RecordDefinition("#stateNum#"           ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering    ,false  ),
                new RecordDefinition("#transitionNum#"      ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering    ,false  ),
                new RecordDefinition("#conditionNum#"       ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering    ,false  ),
                new RecordDefinition("#layerName#"          ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable          ,false  ),
                new RecordDefinition("#statemachinePath#"   ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable          ,false  ),
                new RecordDefinition("#stateName#"          ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable          ,false  ),

                // FIXME:
                // トランジションの持っているコンディションは、全削除、全追加しないと、プロパティ１つ変えられないようだ。（セッターが機能していない）
                // また、コンディションの変更を反映するためには、親トランジションが必要。
                // プロパティを１つずつ変えるのは　処理時間の無駄が膨大だが、　今バージョンはこれでいくものとする。

                // parameter, mode, threshold の順に並べた方が、理解しやすい。
                new RecordDefinition("parameter"            ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None
                    ,(object i)=>{
                        return ((AnimatorConditionWrapper)i).m_source.parameter;
                    }
                    ,(object i,string v)=>{
                        Operation_Condition.UpdateProperty_AndRebuild(
                            ((AnimatorConditionWrapper)i).m_sourceParentTransition,
                            ((AnimatorConditionWrapper)i).ConditionNum,
                            "parameter",
                            v
                            );
                    }
                ),

                // 演算子。本来はイニューム型だが、文字列型にする。
                // 値は本来は Greater,less,Equals,NotEqual,If,IfNot の６つだが、分かりづらいので >, <, =, <>, TRUE, FALSE の６つにする。
                new RecordDefinition("mode"                 ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None
                    ,(object i)=>{ return Mode_to_string(((AnimatorConditionWrapper)i).m_source.mode);}
                    ,(object i,string v)=>{
                        Operation_Condition.UpdateProperty_AndRebuild(
                            ((AnimatorConditionWrapper)i).m_sourceParentTransition,
                            ((AnimatorConditionWrapper)i).ConditionNum,
                            "mode",
                            String_to_mode(v)
                            );
                    }
                ),
                new RecordDefinition("threshold"            ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None
                    ,(object i)=>{ return ((AnimatorConditionWrapper)i).m_source.threshold; }
                    ,(object i,float v)=>{
                         Operation_Condition.UpdateProperty_AndRebuild(
                            ((AnimatorConditionWrapper)i).m_sourceParentTransition,
                            ((AnimatorConditionWrapper)i).ConditionNum,
                            "threshold",
                            v
                            );
                    }
                ),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }

            Empty = new ConditionRecord(-1, -1, -1, -1, -1, new AnimatorCondition());
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static AconObjectRecordable Empty { get; set; }

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
                { "mode"                , Mode_to_string( condition.mode)}, // Change contents
                { "parameter"           , condition.parameter},
                { "threshold"           , condition.threshold},
            };
        }
        public Dictionary<string, object> Fields { get; set; }

        /// <param name="c">コンテンツ contents</param>
        /// <param name="n">列名出力 output column name</param>
        /// <param name="d">列定義出力 output definition</param>
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
    public class PositionRecord : AconObjectRecordable
    {
        /// <summary>
        /// struct を object に渡したいときに使うラッパー
        /// 
        /// ステートマシンと、ステートでは処理が異なる
        /// </summary>
        public class PositionWrapper
        {
            public PositionWrapper(AnimatorStateMachine statemachine, string propertyName)
            {
                m_statemachine = statemachine;
                PropertyName = propertyName;
            }
            public PositionWrapper(AnimatorStateMachine parentStatemachine_ofCaState, ChildAnimatorState caState, string propertyName)
            {
                m_parentStatemachine_ofCaState = parentStatemachine_ofCaState;
                m_caState = caState;
                PropertyName = propertyName;
            }

            public AnimatorStateMachine m_statemachine;
            public AnimatorStateMachine m_parentStatemachine_ofCaState;
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
                            default: throw new UnityException("Not supported. property name = [" + this.PropertyName + "]");
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
                            case "anyStatePosition":
                                this.m_statemachine.anyStatePosition = new Vector3(value, this.m_statemachine.anyStatePosition.y, this.m_statemachine.anyStatePosition.z);
                                break;
                            case "entryPosition":
                                this.m_statemachine.entryPosition = new Vector3(value, this.m_statemachine.entryPosition.y, this.m_statemachine.entryPosition.z);
                                break;
                            case "exitPosition":
                                this.m_statemachine.exitPosition = new Vector3(value, this.m_statemachine.exitPosition.y, this.m_statemachine.exitPosition.z);
                                break;
                            case "parentStateMachinePosition":
                                this.m_statemachine.parentStateMachinePosition = new Vector3(value, this.m_statemachine.parentStateMachinePosition.y, this.m_statemachine.parentStateMachinePosition.z);
                                break;
                            default: throw new UnityException("Not supported. property name = [" + this.PropertyName + "]");
                        }
                    }
                    else {
                        this.m_caState.position = new Vector3(value, this.m_caState.position.y, this.m_caState.position.z);
                    }
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
                            default: throw new UnityException("Not supported. property name = [" + this.PropertyName + "]");
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
                            default: throw new UnityException("Not supported. property name = [" + this.PropertyName + "]");
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
                            default: throw new UnityException("Not supported. property name = [" + this.PropertyName + "]");
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
                            default: throw new UnityException("Not supported. property name = [" + this.PropertyName + "]");
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
                new RecordDefinition("#layerNum#"           ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("#machineStateNum#"    ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("#stateNum#"           ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("#transitionNum#"      ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("#conditionNum#"       ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("#proertyName#"        ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.TemporaryNumbering    ,false),
                new RecordDefinition("#layerName#"          ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable          ,false),
                new RecordDefinition("#statemachinePath#"   ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable          ,false),
                new RecordDefinition("#stateName#"          ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.Identifiable          ,false),
                new RecordDefinition("magnitude"            ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,false),// read only?
                new RecordDefinition("#normalized#"         ,RecordDefinition.FieldType.Other   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,false),
                new RecordDefinition("#normalizedX#"        ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,false),
                new RecordDefinition("#normalizedY#"        ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,false),
                new RecordDefinition("#normalizedZ#"        ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,false),
                new RecordDefinition("sqrMagnitude"         ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.None                  ,false), // read only?
                new RecordDefinition("x"                    ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{ return ((PositionWrapper)i).X; }
                    ,(object i,float v)=>{ ((PositionWrapper)i).X = v; }
                ),
                new RecordDefinition("y"                    ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{ return ((PositionWrapper)i).Y; }
                    ,(object i,float v)=>{ ((PositionWrapper)i).Y = v; }
                ),
                new RecordDefinition("z"                    ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None     ,RecordDefinition.KeyType.UnityEditorDoesNotSupportWriting
                    ,(object i)=>{ return ((PositionWrapper)i).Z; }
                    ,(object i,float v)=>{ ((PositionWrapper)i).Z = v; }
                ),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }

            Empty = new PositionRecord(-1,-1,-1,-1,-1,"",new Vector3());
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static AconObjectRecordable Empty { get; private set; }

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

        /// <param name="c">コンテンツ contents</param>
        /// <param name="n">列名出力 output column name</param>
        /// <param name="d">列定義出力 output definition</param>
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

    /// <summary>
    /// モーション
    /// </summary>
    public class MotionRecord : AconObjectRecordable
    {
        public const string ASSET_PATH = "#assetPath#";

        public class Wrapper
        {
            public Wrapper(Motion source, int countOfAttachDestination)
            {
                Source = source;
                CountOfAttachDestination = countOfAttachDestination;
            }
            public Motion Source { get; private set; }
            public int CountOfAttachDestination { get; set; }
        }

        static MotionRecord()
        {
            List<RecordDefinition> temp = new List<RecordDefinition>()
            {
                new RecordDefinition(ASSET_PATH                     ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None ,RecordDefinition.KeyType.Identifiable  ,false),
                new RecordDefinition("#countOfAttachDestination#"   ,RecordDefinition.FieldType.Int     ,RecordDefinition.SubFieldType.None ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{ return ((Wrapper)i).CountOfAttachDestination; }
                    ,(object i,int v)=>{ ((Wrapper)i).CountOfAttachDestination = v; }
                ),
                new RecordDefinition("apparentSpeed"        ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{ return ((Wrapper)i).Source.apparentSpeed; }
                    ,(object i,float v)=>{ throw new UnityException("Not supported.");}
                ),
                new RecordDefinition("averageAngularSpeed"  ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{ return ((Wrapper)i).Source.averageAngularSpeed; }
                    ,(object i,float v)=>{ throw new UnityException("Not supported.");}
                ),
                new RecordDefinition("averageDuration"      ,RecordDefinition.FieldType.Float   ,RecordDefinition.SubFieldType.None ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{ return ((Wrapper)i).Source.averageDuration; }
                    ,(object i,float v)=>{ throw new UnityException("Not supported.");}
                ),
                new RecordDefinition("#averageSpeed#"       ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{ return ((Wrapper)i).Source.averageSpeed.ToString(); }
                    ,(object i,string v)=>{ throw new UnityException("Not supported."); }
                ),
                new RecordDefinition("#hideFlags_string#"   ,RecordDefinition.FieldType.Other   ,RecordDefinition.SubFieldType.None ,RecordDefinition.KeyType.ReadOnly          ,false),
                new RecordDefinition("isHumanMotion"        ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{ return ((Wrapper)i).Source.isHumanMotion; }
                    ,(object i,bool v)=>{ throw new UnityException("Not supported."); }
                ),
                new RecordDefinition("isLooping"            ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{ return ((Wrapper)i).Source.isLooping; }
                    ,(object i,bool v)=>{ throw new UnityException("Not supported."); }
                ),
                new RecordDefinition("legacy"               ,RecordDefinition.FieldType.Bool    ,RecordDefinition.SubFieldType.None ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{ return ((Wrapper)i).Source.legacy; }
                    ,(object i,bool v)=>{ throw new UnityException("Not supported."); }
                ),
                new RecordDefinition("name"                 ,RecordDefinition.FieldType.String  ,RecordDefinition.SubFieldType.None ,RecordDefinition.KeyType.ReadOnly
                    ,(object i)=>{ return ((Wrapper)i).Source.name; }
                    ,(object i,string v)=>{ ((Wrapper)i).Source.name = v; }
                ),
            };
            Definitions = new Dictionary<string, RecordDefinition>();
            foreach (RecordDefinition def in temp) { Definitions.Add(def.Name, def); }

            Empty = new MotionRecord("", 0, new AnimationClip());
        }
        public static Dictionary<string, RecordDefinition> Definitions { get; private set; }
        public static AconObjectRecordable Empty { get; private set; }

        public MotionRecord(string assetPath, int countOfAttachDestination, Motion motion)
        {
            this.Fields = new Dictionary<string, object>()
            {
                { ASSET_PATH                        , assetPath                         },
                { "#countOfAttachDestination#"      , countOfAttachDestination          }, // 取り付け先の数
                { "apparentSpeed"                   , motion.apparentSpeed              },
                { "averageAngularSpeed"             , motion.averageAngularSpeed        },
                { "averageDuration"                 , motion.averageDuration            },
                { "#averageSpeed#"                  , motion.averageSpeed.ToString()    }, // 文字列化 toString
                { "#hideFlags_string#"              , motion.hideFlags.ToString()       }, // 文字列化 toString
                { "isHumanMotion"                   , motion.isHumanMotion              },
                { "isLooping"                       , motion.isLooping                  },
                { "legacy"                          , motion.legacy                     },
                { "name"                            , motion.name                       },
            };
        }
        public Dictionary<string, object> Fields { get; set; }

        /// <param name="c">コンテンツ contents</param>
        /// <param name="n">列名出力 output column name</param>
        /// <param name="d">列定義出力 output definition</param>
        public void AppendCsvLine(StringBuilder c, bool n, bool d)
        {
            Definitions[ASSET_PATH                  ].AppendCsv(Fields, c, n, d);
            Definitions["#countOfAttachDestination#"].AppendCsv(Fields, c, n, d);
            Definitions["apparentSpeed"             ].AppendCsv(Fields, c, n, d);
            Definitions["averageAngularSpeed"       ].AppendCsv(Fields, c, n, d);
            Definitions["averageDuration"           ].AppendCsv(Fields, c, n, d);
            Definitions["#averageSpeed#"            ].AppendCsv(Fields, c, n, d);
            Definitions["#hideFlags_string#"        ].AppendCsv(Fields, c, n, d);
            Definitions["isHumanMotion"             ].AppendCsv(Fields, c, n, d);
            Definitions["isLooping"                 ].AppendCsv(Fields, c, n, d);
            Definitions["legacy"                    ].AppendCsv(Fields, c, n, d);
            Definitions["name"                      ].AppendCsv(Fields, c, n, d);
            if (n) { c.Append("[EOL],"); }
            if (!d) { c.AppendLine(); }
        }
    }

    /// <summary>
    /// スプレッドシートに並べるデータ
    /// </summary>
    public class AconDocument
    {
        public AconDocument()
        {
            parameters      = new HashSet<ParameterRecord>();
            layers          = new HashSet<LayerRecord>();
            statemachines   = new HashSet<StatemachineRecord>();
            states          = new HashSet<StateRecord>();
            transitions     = new HashSet<TransitionRecord>();
            conditions      = new HashSet<ConditionRecord>();
            positions       = new HashSet<PositionRecord>();
            motions         = new HashSet<MotionRecord>();
        }

        public HashSet<ParameterRecord>     parameters      { get; set; }
        public HashSet<LayerRecord>         layers          { get; set; }
        public HashSet<StatemachineRecord>  statemachines   { get; set; }
        public HashSet<StateRecord>         states          { get; set; }
        public HashSet<TransitionRecord>    transitions     { get; set; }
        public HashSet<ConditionRecord>     conditions      { get; set; }
        public HashSet<PositionRecord>      positions       { get; set; }
        public HashSet<MotionRecord>        motions         { get; set; }
    }

    public abstract class AconDataUtility
    {
        /// <summary>
        /// ハッシュセットの内容の型を変更します。
        ///
        /// 参照 : Cannot convert HashSet to IReadOnlyCollection : http://stackoverflow.com/questions/32762631/cannot-convert-hashset-to-ireadonlycollection
        /// </summary>
        public static HashSet<AconObjectRecordable> ToHash(HashSet<ParameterRecord> src)
        {
            HashSet<AconObjectRecordable> dst = new HashSet<AconObjectRecordable>();
            foreach (ParameterRecord item in src) { dst.Add(item); }
            return dst;
        }
        public static HashSet<AconObjectRecordable> ToHash(HashSet<LayerRecord> src)
        {
            HashSet<AconObjectRecordable> dst = new HashSet<AconObjectRecordable>();
            foreach (LayerRecord item in src) { dst.Add(item); }
            return dst;
        }
        public static HashSet<AconObjectRecordable> ToHash(HashSet<StatemachineRecord> src)
        {
            HashSet<AconObjectRecordable> dst = new HashSet<AconObjectRecordable>();
            foreach (StatemachineRecord item in src) { dst.Add(item); }
            return dst;
        }
        public static HashSet<AconObjectRecordable> ToHash(HashSet<StateRecord> src)
        {
            HashSet<AconObjectRecordable> dst = new HashSet<AconObjectRecordable>();
            foreach (StateRecord item in src) { dst.Add(item); }
            return dst;
        }
        public static HashSet<AconObjectRecordable> ToHash(HashSet<TransitionRecord> src)
        {
            HashSet<AconObjectRecordable> dst = new HashSet<AconObjectRecordable>();
            foreach (TransitionRecord item in src) { dst.Add(item); }
            return dst;
        }
        public static HashSet<AconObjectRecordable> ToHash(HashSet<ConditionRecord> src)
        {
            HashSet<AconObjectRecordable> dst = new HashSet<AconObjectRecordable>();
            foreach (ConditionRecord item in src) { dst.Add(item); }
            return dst;
        }
        public static HashSet<AconObjectRecordable> ToHash(HashSet<PositionRecord> src)
        {
            HashSet<AconObjectRecordable> dst = new HashSet<AconObjectRecordable>();
            foreach (PositionRecord item in src) { dst.Add(item); }
            return dst;
        }
        public static HashSet<AconObjectRecordable> ToHash(HashSet<MotionRecord> src)
        {
            HashSet<AconObjectRecordable> dst = new HashSet<AconObjectRecordable>();
            foreach (MotionRecord item in src) { dst.Add(item); }
            return dst;
        }

        public static void CreateCsvTable(HashSet<AconObjectRecordable> table, AconObjectRecordable empty, bool outputDefinition, StringBuilder contents)
        {
            if (outputDefinition)
            {
                // 列定義シートを作る場合

                // 列定義ヘッダー出力
                RecordDefinition.AppendDefinitionHeader(contents);
                // 列名出力
                empty.AppendCsvLine(contents, false, outputDefinition);
            }
            else
            {
                // 列名出力
                empty.AppendCsvLine(contents, true, outputDefinition);
                foreach (AconObjectRecordable record in table) { record.AppendCsvLine(contents, false, outputDefinition); }
            }
            contents.AppendLine("[EOF],");
        }
    }
}
