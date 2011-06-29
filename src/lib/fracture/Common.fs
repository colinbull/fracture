﻿module Fracture.Common

open System
open System.Net.Sockets
open SocketExtensions
open System.Reflection

/// Creates a Socket and binds it to specified IPEndpoint, if you want a sytem assigned one Use IPEndPoint(IPAddress.Any, 0)
let createSocket (ipendpoint) =
    let socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    socket.Bind(ipendpoint)
    socket

let closeConnection (sock:Socket) =
    try sock.Shutdown(SocketShutdown.Both)
    finally sock.Close()

let send( client:Socket, getsaea:unit -> SocketAsyncEventArgs, completed, maxSize, msg:byte[])= 
    let rec loop offset =
        if offset < msg.Length then
            let ammounttosend =
                let remaining = msg.Length - offset in
                if remaining > maxSize then maxSize else remaining
            let saea = getsaea()
            saea.UserToken <- client
            Buffer.BlockCopy(msg, offset, saea.Buffer, saea.Offset, ammounttosend)
            saea.SetBuffer(saea.Offset, ammounttosend)
            if client.Connected then client.SendAsyncSafe(completed, saea)
                                     loop (offset + ammounttosend)
            else Console.WriteLine(sprintf "Not connected to server")
    loop 0  
    
let aquiredata (args:SocketAsyncEventArgs)= 
    //process received data
    let data:byte[] = Array.zeroCreate args.BytesTransferred
    Buffer.BlockCopy(args.Buffer, args.Offset, data, 0, data.Length)
    data