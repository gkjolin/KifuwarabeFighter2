﻿//
// State Extend
//
using System.Collections.Generic;
using UnityEngine;

namespace StellaQL
{
    public interface StateExRecordable
    {
        /// <summary>
        /// 末尾にドットを含む
        /// </summary>
        string GetBreadCrumb();
        string Name { get; }
        string Fullpath { get; }
        int FullPathHash { get; }
        bool HasFlag_attr(int enumration);
        /// <summary>
        /// ほんとは列挙型にしたい☆
        /// </summary>
        int AttributeEnum { get; }
        /// <summary>
        /// オプション。使わなくても可。
        /// </summary>
        int Cliptype { get; set; }//[CliptypeIndex]
    }

    public abstract class AbstractStateExRecord : StateExRecordable
    {
        public AbstractStateExRecord(string fullpath, int fullpathHash, int attributeEnum)
        {
            Fullpath = fullpath;
            Name = Fullpath.Substring(Fullpath.LastIndexOf('.') + 1); // ドットを含まない
            FullPathHash = fullpathHash;// Animator.StringToHash(Fullpath);
            //Debug.Log("fullpath=["+this.Fullpath+"] name=["+this.Name+"]");
            this.AttributeEnum = attributeEnum;//StateExTable.Attr
        }

        public string GetBreadCrumb() {
            return Fullpath.Substring(0, Fullpath.LastIndexOf('.') + 1);// 末尾にドットを含む
        }
        public string Fullpath { get; set; }
        public string Name { get; set; }
        public int FullPathHash { get; set; }
        public abstract bool HasFlag_attr(int attributeEnumration);
        /// <summary>
        /// ほんとは列挙型にしたい☆
        /// </summary>
        public int AttributeEnum { get; set; }
        /// <summary>
        /// オプション。使わなくても可。
        /// </summary>
        public int Cliptype { get; set; }//[CliptypeIndex]
    }

    public interface StateExTableable
    {
    }


    public abstract class AbstractStateExTable : StateExTableable
    {
        /// <summary>
        /// Animator の state の名前と、AnimationClipの種類の対応☆　手作業で入力しておく（２度手間）
        /// ほんとは キーを ステートインデックス にしたかった。
        /// </summary>
        public Dictionary<int, StateExRecordable> index_to_exRecord;
        /// <summary>
        /// Animator の state の hash を、state番号に変換☆
        /// </summary>
        public Dictionary<int, int> hash_to_index;//<hash,ステートインデックス>
        /// <summary>
        /// TODO: あとでフルパスtoハッシュに置き換える。
        /// </summary>
        public static Dictionary<string, int> fullpath_to_index;


        /// <summary>
        /// シーンの Start( )メソッドで呼び出してください。
        /// </summary>
        public void InsertAllStates()
        {
            hash_to_index = new Dictionary<int, int>(); // <hash,ステートインデックス>
            for (int iState = 0; iState < index_to_exRecord.Count; iState++)
            {
                hash_to_index.Add(Animator.StringToHash(index_to_exRecord[iState].Fullpath), iState);
            }
        }

        /// <summary>
        /// 現在のアニメーター・ステートに対応したデータを取得。
        /// </summary>
        /// <returns></returns>
        public StateExRecordable GetCurrentStateExRecord(Animator animator)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (!this.hash_to_index.ContainsKey(state.fullPathHash))
            {
                throw new UnityException("もしかして列挙型に登録していないステートがあるんじゃないか☆（＾～＾）？　ハッシュは[" + state.fullPathHash + "]だぜ☆");
            }

            int stateIndex = this.hash_to_index[state.fullPathHash];

            if (this.index_to_exRecord.ContainsKey(stateIndex))
            {
                StateExRecordable stateExRecord = this.index_to_exRecord[stateIndex];
                //((AbstractStateExRecord)stateExRecord).FullPathHash = state.fullPathHash;
                return stateExRecord;
            }

            throw new UnityException("[" + stateIndex + "]のデータが無いぜ☆　なんでだろな☆？（＾～＾）");
        }

        /// <summary>
        /// 現在のアニメーション・クリップに対応したデータを取得。
        /// </summary>
        /// <returns></returns>
        public CliptypeExRecordable GetCurrentCliptypeExRecord(Animator animator, CliptypeExTableable cliptypeExTable)
        {
            AnimatorStateInfo animeStateInfo = animator.GetCurrentAnimatorStateInfo(0);

            //CliptypeIndex
            int cliptype = (index_to_exRecord[hash_to_index[animeStateInfo.fullPathHash]]).Cliptype;

            if (index_to_exRecord.ContainsKey(cliptype))
            {
                return cliptypeExTable.Index_to_exRecord[cliptype];
            }

            throw new UnityException("cliptype = [" + cliptype + "]に対応するアニメーション・クリップのレコードが無いぜ☆");
        }

        /// <summary>
        /// 属性検索
        /// </summary>
        public List<StateExRecordable> Where(int enumration_attr)
        {
            List<StateExRecordable> recordset = new List<StateExRecordable>();

            foreach (StateExRecordable record in index_to_exRecord.Values)
            {
                if (record.HasFlag_attr(enumration_attr)) // if(attribute.HasFlag(record.attribute))
                {
                    recordset.Add(record);
                }
            }

            return recordset;
        }

    }
}
