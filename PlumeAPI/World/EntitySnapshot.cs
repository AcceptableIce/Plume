﻿using Lidgren.Network;
using Microsoft.Xna.Framework;
using PlumeAPI.Entities;
using PlumeAPI.Entities.Components;
using PlumeAPI.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlumeAPI.World {
	public class EntitySnapshot {
		public int Id;
		HashSet<SyncablePropertyValue> ChangedProperties = new HashSet<SyncablePropertyValue>();
		Dictionary<string, object> AllProperties = new Dictionary<string, object>();

		static Dictionary<Type, Action<object, OutgoingMessage>> MessageWriters = new Dictionary<Type, Action<object, OutgoingMessage>>();
		static Dictionary<Type, Func<IncomingMessage, object>> MessageReaders = new Dictionary<Type, Func<IncomingMessage, object>>();

		public EntitySnapshot(BaseEntity entity) {
			Id = entity.Id;


			NetworkedPropertyData[] properties = entity.GetDerivativeComponent<NetworkedComponent>().GetSyncableProperties();
			int i = 0;
			foreach(NetworkedPropertyData property in properties) {
				Console.WriteLine(property.Referencer.Info.Name + "," + property.Dirty);
				if(property.Dirty) {
					ChangedProperties.Add(new SyncablePropertyValue(i, property.Get()));
					property.Dirty = false;
				}
				i++;
			}
		}

		public void AddToMessage(OutgoingMessage message) {
			if(ChangedProperties.Count() > 0) {
				message.Write(Id);
				message.Write((byte)ChangedProperties.Count());
				foreach(SyncablePropertyValue property in ChangedProperties) {
					try {
						message.Write((byte)property.Position);
						MessageWriters[property.Value.GetType()].Invoke(property.Value, message);
					} catch(KeyNotFoundException) {
						Console.WriteLine("Unable to parse syncable property: no valid message writer for type " + property.Value.GetType().FullName + ".");
					}
				}
			}
		}

		public static Action<object, OutgoingMessage> GetMessageWriter(Type type) {
			return MessageWriters[type];
		}

		public static void RegisterTypeHandler(Type type, Func<IncomingMessage, object> incoming, Action<object, OutgoingMessage> outgoing) {
			MessageReaders.Add(type, incoming);
			MessageWriters.Add(type, outgoing);
		}

		public static object ReadType(Type type, IncomingMessage message) {
			try {
				return MessageReaders[type].Invoke(message);
			} catch(KeyNotFoundException) {
				Console.WriteLine("Unable to parse syncable property " + type + ": no valid message reader for type " + type + ".");
				return null;
			}
		}

		static EntitySnapshot() {
			RegisterTypeHandler(
				typeof(System.Byte),
				(message) => { return message.ReadByte(); },
				(obj, message) => { message.Write((byte)obj); }
			);

			RegisterTypeHandler(
				typeof(System.Int16),
				(message) => { return message.ReadInt16(); },
				(obj, message) => { message.Write((short)obj); }
			);

			RegisterTypeHandler(
				typeof(System.Int32),
				(message) => { return message.ReadInt32(); },
				(obj, message) => { message.Write((int)obj); }
			);

			RegisterTypeHandler(
				typeof(System.Int64),
				(message) => { return message.ReadInt64(); },
				(obj, message) => { message.Write((long)obj); }
			);

			RegisterTypeHandler(
				typeof(System.String),
				(message) => { return message.ReadString(); },
				(obj, message) => { message.Write((string)obj); }
			);

			RegisterTypeHandler(
				typeof(System.Single),
				(message) => { return message.ReadFloat(); },
				(obj, message) => { message.Write((float)obj); }
			);

			RegisterTypeHandler(
				typeof(System.Double),
				(message) => { return message.ReadDouble(); },
				(obj, message) => { message.Write((double)obj); }
			);

			RegisterTypeHandler(
				typeof(System.Boolean),
				(message) => { return message.ReadBoolean(); },
				(obj, message) => { message.Write((bool)obj); }
			);

			RegisterTypeHandler(
				typeof(Microsoft.Xna.Framework.Vector2),
				(message) => { return new Vector2(message.ReadFloat(), message.ReadFloat()); },
				(obj, message) => {
					Vector2 vec2 = (Vector2)obj;
					message.Write(vec2.X);
					message.Write(vec2.Y);
				}
			);
		}
	}

	struct SyncablePropertyValue {
		public int Position;
		public object Value;
		public SyncablePropertyValue(int position, object value) {
			Position = position;
			Value = value;
		}
	}

}
