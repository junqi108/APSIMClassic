//---------------------------------------------------------------------------
#ifndef MessageDataH
#define MessageDataH

#include "message.h"
#include <string>
#include <vector>
#include <General/string_functions.h>
#ifdef __WIN32__
   #include <mem.h>
#endif

// ------------------------------------------------------------------
// Provides a mechanism to read and write values to the data
// section of a message.
// ------------------------------------------------------------------
class MessageData
   {
   private:
      const char* startBuffer;
      char* currentPtr;
      unsigned int bufferSize;
   public:
      MessageData(void) : startBuffer(NULL), currentPtr(NULL), bufferSize(0) { }
      MessageData(char* dataPtr, unsigned int numBytes)
         : startBuffer((char*)dataPtr),
           currentPtr(dataPtr),
           bufferSize(numBytes)
         {
         }
      MessageData(const Message& msg)
         : startBuffer((char*)msg.dataPtr),
           currentPtr(msg.dataPtr),
           bufferSize(msg.nDataBytes)
         {
         }

      void reset(void) {currentPtr = (char*)startBuffer;}
      bool isValid(void) const
         {
         return (startBuffer != NULL && currentPtr != NULL
                 && bytesRead() < totalBytes());
         }
      char* ptr(void) const {return currentPtr;}
      const char* start(void) const {return startBuffer;}
      void seek(char* ptr) {currentPtr = ptr;}
      unsigned totalBytes(void)  const {return bufferSize;}
      unsigned bytesRead(void)   const {return currentPtr-startBuffer;}
      unsigned bytesUnRead(void) const
         {return totalBytes()-bytesRead();}

      void movePtrBy(unsigned int numBytes)
         {
         currentPtr += numBytes;
         }
      void copyFrom(const char* from, unsigned int numBytes)
         {
         memcpy(currentPtr, from, numBytes);
         currentPtr += numBytes;
         }

   };

// ------------------------------------------------------------------
// Creates a new message, packing the specified data automatically.
// ------------------------------------------------------------------
template <class T>
Message& newMessage(Message::Type messageType,
                    unsigned int fromID,
                    unsigned int toID,
                    bool acknowledgementRequired,
                    T& data)
   {
   Message& msg = constructMessage(messageType, fromID, toID, acknowledgementRequired,
                                   memorySize(data));
   MessageData messageData(msg);
   pack(messageData, data);
   return msg;
   }


#endif
