using System;using System.Threading;using System.Security.AccessControl;using System.Security.Principal;
public static class RunGate {
 public static Mutex Open(){var security=new MutexSecurity();security.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid,null),MutexRights.FullControl,AccessControlType.Allow));bool created;return new Mutex(false,"Global\\DelProHdxScheduledWrite",out created,security);}
 public static bool TryEnter(Mutex gate){try{return gate.WaitOne(0);}catch(AbandonedMutexException){return true;}}
}

