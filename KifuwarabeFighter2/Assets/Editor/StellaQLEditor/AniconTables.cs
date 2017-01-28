﻿//
// Animation Controller Tables
//
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.Animations;
using UnityEngine;

namespace StellaQL
{
    /// <summary>
    /// レイヤー
    /// </summary>
    public class LayerRecord
    {
        #region プロパティー
        /// <summary>
        /// レイヤー行番号
        /// </summary>
        public int layerNum;

        /// <summary>
        /// レイヤー名
        /// </summary>
        public string name;

        /// <summary>
        /// 略
        /// </summary>
        public string avatarMask;

        /// <summary>
        /// 略
        /// </summary>
        public string blendingMode;

        /// <summary>
        /// 
        /// </summary>
        public float defaultWeight;

        /// <summary>
        /// 
        /// </summary>
        public bool iKPass;

        /// <summary>
        /// 
        /// </summary>
        public bool syncedLayerAffectsTiming;

        /// <summary>
        /// 
        /// </summary>
        public int syncedLayerIndex;
        #endregion

        public LayerRecord(int num, AnimatorControllerLayer layer)
        {
            layerNum = num;
            name = layer.name;
            avatarMask = layer.avatarMask == null ? "" : layer.avatarMask.ToString();
            blendingMode = layer.blendingMode.ToString();
            defaultWeight = layer.defaultWeight;
            iKPass = layer.iKPass;
            syncedLayerAffectsTiming = layer.syncedLayerAffectsTiming;
            syncedLayerIndex = layer.syncedLayerIndex;
        }

        public void CreateCsvLine(StringBuilder contents)
        {
            contents.Append(layerNum); contents.Append(","); // レイヤー行番号
            contents.Append(CsvParser.EscapeCell( name)); contents.Append(","); // レイヤー名
            contents.Append(CsvParser.EscapeCell(avatarMask)); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(blendingMode)); contents.Append(",");
            contents.Append(defaultWeight); contents.Append(",");
            contents.Append(iKPass); contents.Append(",");
            contents.Append(syncedLayerAffectsTiming); contents.Append(",");
            contents.Append(syncedLayerIndex); contents.Append(",");
            contents.AppendLine();
        }

        public static void ColumnNameCsvLine(StringBuilder contents) { contents.AppendLine( "LayerNum,LayerName,avatarMask,blendingMode,defaultWeight,iKPass,syncedLayerAffectsTiming,syncedLayerIndex,"); }
    }

    /// <summary>
    /// ステートマシン
    /// </summary>
    public class StatemachineRecord
    {
        public int layerNum;
        public int machineStateNum;
        public string anyStateTransitions;
        public string behaviours;
        public string defaultState;
        public string entryTransitions;
        public string hideFlags;
        public string name;

        public StatemachineRecord(int layerNum, int machineStateNum, AnimatorStateMachine stateMachine, List<PositionRecord> positionsTable)
        {
            this.layerNum = layerNum;
            this.machineStateNum = machineStateNum;

            if (stateMachine.anyStatePosition != null)
            {
                positionsTable.Add(new PositionRecord(layerNum, machineStateNum, -1, -1, -1, "anyStatePosition", stateMachine.anyStatePosition));
            }

            anyStateTransitions = stateMachine.anyStateTransitions == null ? "" : stateMachine.anyStateTransitions.ToString();
            behaviours = stateMachine.behaviours == null ? "" : stateMachine.behaviours.ToString();
            defaultState = stateMachine.defaultState == null ? "" : stateMachine.defaultState.ToString();

            if (stateMachine.entryPosition != null)
            {
                positionsTable.Add(new PositionRecord(layerNum, machineStateNum, -1, -1, -1, "entryPosition", stateMachine.entryPosition));
                //entryPosition = stateMachine.entryPosition == null ? "" : stateMachine.entryPosition.ToString();
            }

            entryTransitions = stateMachine.entryTransitions == null ? "" : stateMachine.entryTransitions.ToString();

            if (stateMachine.exitPosition != null)
            {
                positionsTable.Add(new PositionRecord(layerNum, machineStateNum, -1, -1, -1, "exitPosition", stateMachine.exitPosition));
                //exitPosition = stateMachine.exitPosition == null ? "" : stateMachine.exitPosition.ToString();
            }

            hideFlags = stateMachine.hideFlags.ToString();
            name = stateMachine.name;

            if (stateMachine.parentStateMachinePosition != null)
            {
                positionsTable.Add(new PositionRecord(layerNum, machineStateNum, -1, -1, -1, "parentStateMachinePosition", stateMachine.parentStateMachinePosition));
                //parentStateMachinePosition = stateMachine.parentStateMachinePosition == null ? "" : stateMachine.parentStateMachinePosition.ToString();
            }
        }

        public void CreateCsvLine(StringBuilder contents)
        {
            contents.Append(layerNum); contents.Append(",");
            contents.Append(machineStateNum); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(anyStateTransitions)); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(behaviours)); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(defaultState)); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(entryTransitions)); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(hideFlags)); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(name)); contents.Append(",");
            contents.AppendLine();
        }

        public static void ColumnNameCsvLine(StringBuilder contents) { contents.AppendLine( "LayerNum,MachineStateNum,anyStateTransitions,behaviours,defaultState,entryTransitions,hideFlags,name,"); }
    }

    /// <summary>
    /// ステート
    /// </summary>
    public class StateRecord
    {
        #region プロパティー
        public int layerNum;
        public int machineStateNum;
        public int stateNum;

        //
        public float cycleOffset;
        public string cycleOffsetParameter;
        public string hideFlags;
        public bool iKOnFeet;
        public bool mirror;
        public string mirrorParameter;
        public bool mirrorParameterActive;
        public string motion_name;
        public string name;
        public int nameHash;
        public float speed;
        public string speedParameter;
        public bool speedParameterActive;
        public string tag;
        public bool writeDefaultValues;
        #endregion

        public static StateRecord CreateInstance(int layerNum, int machineStateNum, int stateNum, ChildAnimatorState caState, List<PositionRecord> positionsTable)
        {
            positionsTable.Add(new PositionRecord(layerNum, machineStateNum, stateNum, -1, -1, "position", caState.position));
            return new StateRecord(layerNum, machineStateNum, stateNum, caState.state);
        }
        public StateRecord(int layerNum, int machineStateNum, int stateNum, AnimatorState state)
        {
            this.layerNum = layerNum;
            this.machineStateNum = machineStateNum;
            this.stateNum = stateNum;

            cycleOffset = state.cycleOffset;
            cycleOffsetParameter = state.cycleOffsetParameter;
            hideFlags = state.hideFlags.ToString();
            iKOnFeet = state.iKOnFeet;
            mirror = state.mirror;
            mirrorParameter = state.mirrorParameter;
            mirrorParameterActive = state.mirrorParameterActive;
            motion_name = state.motion == null ? "" : state.motion.name; // とりあえず名前だけ☆
            name = state.name;
            nameHash = state.nameHash;
            speed = state.speed;
            speedParameter = state.speedParameter;
            speedParameterActive = state.speedParameterActive;
            tag = state.tag;
            writeDefaultValues = state.writeDefaultValues;
        }

        public void CreateCsvLine(StringBuilder contents)
        {
            contents.Append(layerNum); contents.Append(",");
            contents.Append(machineStateNum); contents.Append(",");
            contents.Append(stateNum); contents.Append(",");

            //
            contents.Append(cycleOffset); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(cycleOffsetParameter)); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(hideFlags)); contents.Append(",");
            contents.Append(iKOnFeet); contents.Append(",");
            contents.Append(mirror); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(mirrorParameter)); contents.Append(",");
            contents.Append(mirrorParameterActive); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(motion_name)); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(name)); contents.Append(",");
            contents.Append(nameHash); contents.Append(",");
            contents.Append(speed); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(speedParameter)); contents.Append(",");
            contents.Append(speedParameterActive); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(tag)); contents.Append(",");
            contents.Append(writeDefaultValues); contents.Append(",");
            contents.AppendLine();
        }

        public static void ColumnNameCsvLine(StringBuilder contents) { contents.AppendLine("LayerNum,MachineStateNum,StateNum,cycleOffset,cycleOffsetParameter,hideFlags,iKOnFeet,mirror,mirrorParameter,mirrorParameterActive,motion_name,name,nameHash,speed,speedParameter,speedParameterActive,tag,writeDefaultValues,"); }
    }

    /// <summary>
    /// トランジション
    /// ※コンディションは別テーブル
    /// </summary>
    public class TransitionRecord
    {
        #region プロパティー
        public int layerNum;
        public int machineStateNum;
        public int stateNum;
        public int transitionNum;
        public bool canTransitionToSelf;
        public string destinationState_name;
        public int destinationState_nameHash;
        public string destinationStateMachine_name;
        public float duration;
        public float exitTime;
        public bool hasExitTime;
        public bool hasFixedDuration;
        public string hideFlags;
        public string interruptionSource;
        public bool isExit;
        public bool mute;
        public string name;
        public float offset;
        public bool orderedInterruption;
        public bool solo;
        public string stellaQLComment;
        #endregion

        public TransitionRecord(int layerNum, int machineStateNum, int stateNum, int transitionNum, AnimatorStateTransition transition, string stellaQLComment)
        {
            this.layerNum = layerNum;
            this.machineStateNum = machineStateNum;
            this.stateNum = stateNum;
            this.transitionNum = transitionNum;
            this.stellaQLComment = stellaQLComment;

            canTransitionToSelf = transition.canTransitionToSelf;
            //conditions = transition.conditions.ToString();

            // 名前のみ取得
            destinationState_name = transition.destinationState == null ? "" : transition.destinationState.name;
            destinationState_nameHash = transition.destinationState == null ? 0 : transition.destinationState.nameHash;

            // 名前のみ取得
            destinationStateMachine_name = transition.destinationStateMachine == null ? "" : transition.destinationStateMachine.name;

            duration = transition.duration;
            exitTime = transition.exitTime;
            hasExitTime = transition.hasExitTime;
            hasFixedDuration = transition.hasFixedDuration;
            hideFlags = transition.hideFlags.ToString();
            interruptionSource = transition.interruptionSource.ToString();
            isExit = transition.isExit;
            mute = transition.mute;
            name = transition.name;
            offset = transition.offset;
            orderedInterruption = transition.orderedInterruption;
            solo = transition.solo;
        }

        public void CreateCsvLine(StringBuilder contents)
        {
            contents.Append(layerNum); contents.Append(",");
            contents.Append(machineStateNum); contents.Append(",");
            contents.Append(stateNum); contents.Append(",");
            contents.Append(transitionNum); contents.Append(",");

            contents.Append(canTransitionToSelf); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(destinationState_name)); contents.Append(",");
            contents.Append(destinationState_nameHash); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(destinationStateMachine_name)); contents.Append(",");
            contents.Append(duration); contents.Append(",");
            contents.Append(exitTime); contents.Append(",");
            contents.Append(hasExitTime); contents.Append(",");
            contents.Append(hasFixedDuration); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(hideFlags)); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(interruptionSource)); contents.Append(",");
            contents.Append(isExit); contents.Append(",");
            contents.Append(mute); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(name)); contents.Append(",");
            contents.Append(offset); contents.Append(",");
            contents.Append(orderedInterruption); contents.Append(",");
            contents.Append(solo); contents.Append(",");
            contents.AppendLine();
        }

        public static void ColumnNameCsvLine(StringBuilder contents) { contents.AppendLine("LayerNum,MachineStateNum,StateNum,TransitionNum,canTransitionToSelf,destinationState_name,destinationState_nameHash,destinationStateMachine,duration,exitTime,hasExitTime,hasFixedDuration,hideFlags,interruptionSource,isExit,mute,name,offset,orderedInterruption,solo,"); }
    }

    /// <summary>
    /// コンディション
    /// </summary>
    public class ConditionRecord
    {
        public int layerNum;
        public int machineStateNum;
        public int stateNum;
        public int transitionNum;
        public int conditionNum;
        public string mode;
        public string parameter;
        public float threshold;

        public ConditionRecord(int layerNum, int machineStateNum, int stateNum, int transitionNum, int conditionNum, AnimatorCondition condition)
        {
            this.layerNum = layerNum;
            this.machineStateNum = machineStateNum;
            this.stateNum = stateNum;
            this.transitionNum = transitionNum;
            this.conditionNum = conditionNum;
            mode = condition.mode.ToString();
            parameter = condition.parameter;
            threshold = condition.threshold;
        }

        public void CreateCsvLine(StringBuilder contents)
        {
            contents.Append(layerNum); contents.Append(",");
            contents.Append(machineStateNum); contents.Append(",");
            contents.Append(stateNum); contents.Append(",");
            contents.Append(transitionNum); contents.Append(",");
            contents.Append(conditionNum); contents.Append(",");

            contents.Append(CsvParser.EscapeCell(mode)); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(parameter)); contents.Append(",");
            contents.Append(threshold); contents.Append(",");
            contents.AppendLine();
        }

        public static void ColumnNameCsvLine(StringBuilder contents) { contents.AppendLine( "LayerNum,MachineStateNum,StateNum,TransitionNum,ConditionNum,mode,parameter,threshold,"); }
    }

    /// <summary>
    /// ポジション
    /// </summary>
    public class PositionRecord
    {
        #region プロパティー
        public int layerNum;
        public int machineStateNum;
        public int stateNum;
        public int transitionNum;
        public int conditionNum;
        public string proertyName;

        //
        public float magnitude;
        public string normalized;
        public float normalizedX;
        public float normalizedY;
        public float normalizedZ;
        public float sqrMagnitude;
        public float x;
        public float y;
        public float z;
        #endregion

        public PositionRecord(
            int layerNum,
            int machineStateNum,
            int stateNum,
            int transitionNum,
            int conditionNum,
            string proertyName,
            Vector3 position)
        {
            this.layerNum = layerNum;
            this.machineStateNum = machineStateNum;
            this.stateNum = stateNum;
            this.transitionNum = transitionNum;
            this.conditionNum = conditionNum;
            this.proertyName = proertyName;

            magnitude = position.magnitude;
            //normalized = position.normalized == null ? "" : "(解析未対応)";
            normalized = position.normalized == null ? "" : position.normalized.ToString();
            //normalized = position.normalized == null ? "" : Util_CsvParser.CellList_to_csvLine( Util_CsvParser.CsvLine_to_cellList(position.normalized.ToString()));
            normalizedX = position.x;
            normalizedY = position.y;
            normalizedZ = position.z;
            sqrMagnitude = position.sqrMagnitude;
            x = position.x;
            y = position.y;
            z = position.z;
        }

        public void CreateCsvLine(StringBuilder contents)
        {
            contents.Append(layerNum); contents.Append(",");
            contents.Append(machineStateNum); contents.Append(",");
            contents.Append(stateNum); contents.Append(",");
            contents.Append(transitionNum); contents.Append(",");
            contents.Append(conditionNum); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(proertyName)); contents.Append(",");

            contents.Append(magnitude); contents.Append(",");
            contents.Append(CsvParser.EscapeCell(normalized)); contents.Append(",");
            contents.Append(normalizedX); contents.Append(",");
            contents.Append(normalizedY); contents.Append(",");
            contents.Append(normalizedZ); contents.Append(",");
            contents.Append(sqrMagnitude); contents.Append(",");
            contents.Append(x); contents.Append(",");
            contents.Append(y); contents.Append(",");
            contents.Append(z); contents.Append(",");
            contents.AppendLine();
        }

        public static void ColumnNameCsvLine(StringBuilder contents) { contents.AppendLine( "LayerNum,MachineStateNum,StateNum,TransitionNum,ConditionNum,PropertyName,magnitude,normalized,normalizedX,normalizedY,normalizedZ,sqrMagnitude,x,y,z,"); }
    }

    public abstract class AniconTables
    {
        public static List<LayerRecord> table_layer = new List<LayerRecord>();
        public static List<StatemachineRecord> table_statemachine = new List<StatemachineRecord>();
        public static HashSet<StateRecord> table_state = new HashSet<StateRecord>();
        public static HashSet<TransitionRecord> table_transition = new HashSet<TransitionRecord>();
        public static List<ConditionRecord> table_condition = new List<ConditionRecord>();
        public static List<PositionRecord> table_position = new List<PositionRecord>();

        public static void WriteCsv_Parameters(AnimatorController ac, StringBuilder message)
        {
            message.AppendLine("Parameters Scanning...☆（＾～＾）");

            StringBuilder contents = new StringBuilder();
            // 見出し列
            contents.AppendLine("Num,Name,Bool,Float,Int,NameHash");

            AnimatorControllerParameter[] acpArray = ac.parameters;
            int num = 0;
            foreach (AnimatorControllerParameter acp in acpArray)
            {
                contents.Append(num);
                contents.Append(",");
                contents.Append(acp.name);
                contents.Append(",");
                contents.Append(acp.defaultBool);
                contents.Append(",");
                contents.Append(acp.defaultFloat);
                contents.Append(",");
                contents.Append(acp.defaultInt);
                contents.Append(",");
                contents.Append(acp.nameHash);
                contents.Append(",");

                contents.AppendLine();
                num++;
            }

            StellaQLWriter.Write(StellaQLWriter.Filepath_LogParameters(ac.name), contents, message);
        }

        public static void WriteCsv_Layer(string aniconName, StringBuilder message)
        {
            StringBuilder contents = new StringBuilder();
            LayerRecord.ColumnNameCsvLine(contents);
            foreach (LayerRecord record in table_layer) {record.CreateCsvLine(contents);}
            StellaQLWriter.Write(StellaQLWriter.Filepath_LogLayer(aniconName), contents, message);
        }

        public static void WriteCsv_Statemachine(string aniconName, StringBuilder message)
        {
            StringBuilder contents = new StringBuilder();
            StatemachineRecord.ColumnNameCsvLine(contents);
            foreach (StatemachineRecord record in table_statemachine) {record.CreateCsvLine(contents);}
            StellaQLWriter.Write(StellaQLWriter.Filepath_LogStatemachine(aniconName), contents, message);
        }

        public static void CreateCsvTable_State(HashSet<StateRecord> table, StringBuilder contents)
        {
            StateRecord.ColumnNameCsvLine(contents);
            foreach (StateRecord stateRecord in table) { stateRecord.CreateCsvLine(contents); }
        }
        public static void WriteCsv_State(string aniconName, StringBuilder message)
        {
            StringBuilder contents = new StringBuilder();
            CreateCsvTable_State(table_state, contents);
            StellaQLWriter.Write(StellaQLWriter.Filepath_LogStates(aniconName), contents, message);
        }

        public static void CreateCsvTable_Transition(HashSet<TransitionRecord> table, StringBuilder contents)
        {
            TransitionRecord.ColumnNameCsvLine(contents);
            foreach (TransitionRecord record in table) { record.CreateCsvLine(contents); }
        }
        public static void WriteCsv_Transition(string aniconName, StringBuilder message)
        {
            StringBuilder contents = new StringBuilder();
            CreateCsvTable_Transition(table_transition, contents);
            StellaQLWriter.Write(StellaQLWriter.Filepath_LogTransition(aniconName), contents, message);
        }

        public static void WriteCsv_Condition(string aniconName, StringBuilder message)
        {
            StringBuilder contents = new StringBuilder();
            ConditionRecord.ColumnNameCsvLine(contents);
            foreach (ConditionRecord conditionRecord in table_condition) { conditionRecord.CreateCsvLine(contents); }
            StellaQLWriter.Write(StellaQLWriter.Filepath_LogConditions(aniconName), contents, message);
        }

        public static void WriteCsv_Position(string aniconName, StringBuilder message)
        {
            StringBuilder contents = new StringBuilder();
            PositionRecord.ColumnNameCsvLine(contents);
            foreach (PositionRecord record in table_position) { record.CreateCsvLine(contents); }
            StellaQLWriter.Write(StellaQLWriter.Filepath_LogPositions(aniconName), contents, message);
        }

        private static void ScanRecursive(List<AnimatorStateMachine> aStateMachineList, AnimatorStateMachine stateMachine)
        {
            aStateMachineList.Add(stateMachine);

            foreach (ChildAnimatorStateMachine caStateMachine in stateMachine.stateMachines)
            {
                ScanRecursive(aStateMachineList, caStateMachine.stateMachine);
            }
        }

        public static void ScanAnimatorController(AnimatorController ac, StringBuilder message)
        {
            message.AppendLine("States Scanning...☆（＾～＾）");
            table_layer.Clear();
            table_state.Clear();
            table_transition.Clear();
            table_condition.Clear();
            table_position.Clear();

            foreach (AnimatorControllerLayer layer in ac.layers)//レイヤー
            {
                LayerRecord layerRecord = new LayerRecord(table_layer.Count, layer);
                table_layer.Add(layerRecord);

                // ステート・マシン
                List<AnimatorStateMachine> stateMachineList = new List<AnimatorStateMachine>();
                ScanRecursive(stateMachineList, layer.stateMachine);
                foreach (AnimatorStateMachine stateMachine in stateMachineList)
                {
                    StatemachineRecord stateMachineRecord = new StatemachineRecord(table_layer.Count, table_statemachine.Count, stateMachine, table_position);
                    table_statemachine.Add(stateMachineRecord);

                    foreach (ChildAnimatorState caState in stateMachine.states)
                    {
                        StateRecord stateRecord = StateRecord.CreateInstance(table_layer.Count, table_statemachine.Count, table_state.Count, caState, table_position);
                        table_state.Add(stateRecord);

                        foreach (AnimatorStateTransition transition in caState.state.transitions)
                        {
                            TransitionRecord transitionRecord = new TransitionRecord(table_layer.Count, table_statemachine.Count, table_state.Count, table_transition.Count, transition, "");
                            table_transition.Add(transitionRecord);

                            foreach (AnimatorCondition aniCondition in transition.conditions)
                            {
                                ConditionRecord conditionRecord = new ConditionRecord(table_layer.Count, table_statemachine.Count, table_state.Count, table_transition.Count, table_condition.Count, aniCondition);
                                table_condition.Add(conditionRecord);
                            } // コンディション
                        }//トランジション
                    }//ステート
                }

            }//レイヤー

            message.AppendLine( "Scanned☆（＾▽＾）");
        }

    }
}
