using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;


namespace ZT.FCL
{
    /// <summary>
    /// 说明：用于在各层之间传递结果的消息
    /// 作者：李浩
    /// 时间：2013-12-23 10：41：22    
    /// </summary>
    [Serializable]
    [DataContract]
    public class ReturnValue
    {
        /// <summary>
        /// 成功消息
        /// </summary>
        private string _message;


        /// <summary>
        /// 状态：成功、失败
        /// </summary>
        [DataMember]
        public bool State 
        { 
            get; 
            protected set; 
        }

        /// <summary>
        /// 结果消息
        /// </summary>
        [DataMember]
        public string Message
        {
            get 
            {
                return this.State ? this._message : this.Error.ShortMessage;
            }
            protected set
            { 
                this._message = value;
            }
        }

        /// <summary>
        /// 返回请求处理中的错误对象
        /// </summary>
        [DataMember]
        public ErrorCode Error { get; set; }         
      

        /// <summary>
        /// 构造器
        /// </summary>
        public ReturnValue()
        {
            this.State = false;
            this.Message = string.Empty;
        }

        /// <summary>
        /// 设置成功
        /// </summary>
        /// <param name="message">成功消息</param>
        public void Success(string message = "")
        {
            this.State = true;
            this.Message = message;
        }

        /// <summary>
        /// 设置失败
        /// </summary>
        /// <param name="message">失败消息</param>
        public void Fail(string message = "")
        {
            this.State = false;
            this.Error = new ErrorCode(string.Empty, message);
        }

        /// <summary>
        /// 设置失败
        /// </summary>
        /// <param name="error">错误对象</param>
        public void Fail(ErrorCode error)
        {
            this.State = false;
            this.Error = error;
        }
    }
}
