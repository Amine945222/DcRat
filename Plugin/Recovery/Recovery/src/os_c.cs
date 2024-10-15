using System.Diagnostics;
using System.Text;
using HANDLE = System.IntPtr;
using i64 = System.Int64;
using u32 = System.UInt32;


namespace CS_SQLite3
{
    public partial class CSSQLite
    {
        /*
         ** 2005 November 29
         **
         ** The author disclaims copyright to this source code.  In place of
         ** a legal notice, here is a blessing:
         **
         **    May you do good and not evil.
         **    May you find forgiveness for yourself and forgive others.
         **    May you share freely, never taking more than you give.
         **
         ******************************************************************************
         **
         ** This file contains OS interface code that is common to all
         ** architectures.
         **
         ** $Id: os.c,v 1.127 2009/07/27 11:41:21 danielk1977 Exp $
         **
         *************************************************************************
         **  Included in SQLite3 port to C#-SQLite;  2008 Noah B Hart
         **  C#-SQLite is an independent reimplementation of the SQLite software library
         **
         **  $Header$
         *************************************************************************
         */
        //#define _SQLITE_OS_C_ 1
        //#include "sqliteInt.h"
        //#undef _SQLITE_OS_C_

        /*
         ** The default SQLite sqlite3_vfs implementations do not allocate
         ** memory (actually, os_unix.c allocates a small amount of memory
         ** from within OsOpen()), but some third-party implementations may.
         ** So we test the effects of a malloc() failing and the sqlite3OsXXX()
         ** function returning SQLITE_IOERR_NOMEM using the DO_OS_MALLOC_TEST macro.
         **
         ** The following functions are instrumented for malloc() failure
         ** testing:
         **
         **     sqlite3OsOpen()
         **     sqlite3OsRead()
         **     sqlite3OsWrite()
         **     sqlite3OsSync()
         **     sqlite3OsLock()
         **
         */
#if (SQLITE_TEST) && !SQLITE_OS_WIN
//#define DO_OS_MALLOC_TEST(x) if (!x || !sqlite3IsMemJournal(x)) {     \
void *pTstAlloc = sqlite3Malloc(10);                             \
if (!pTstAlloc) return SQLITE_IOERR_NOMEM;                       \
//sqlite3_free(pTstAlloc);                                         \
}
#else
        //#define DO_OS_MALLOC_TEST(x)
        private static void DO_OS_MALLOC_TEST(sqlite3_file x)
        {
        }
#endif


        /*
         ** The following routines are convenience wrappers around methods
         ** of the sqlite3_file object.  This is mostly just syntactic sugar. All
         ** of this would be completely automatic if SQLite were coded using
         ** C++ instead of plain old C.
         */
        private static int sqlite3OsClose(sqlite3_file pId)
        {
            var rc = SQLITE_OK;
            if (pId.pMethods != null)
            {
                rc = pId.pMethods.xClose(pId);
                pId.pMethods = null;
            }

            return rc;
        }

        private static int sqlite3OsRead(sqlite3_file id, byte[] pBuf, int amt, i64 offset)
        {
            DO_OS_MALLOC_TEST(id);
            if (pBuf == null) pBuf = new byte[amt];
            return id.pMethods.xRead(id, pBuf, amt, offset);
        }

        private static int sqlite3OsWrite(sqlite3_file id, byte[] pBuf, int amt, i64 offset)
        {
            DO_OS_MALLOC_TEST(id);
            return id.pMethods.xWrite(id, pBuf, amt, offset);
        }

        private static int sqlite3OsTruncate(sqlite3_file id, i64 size)
        {
            return id.pMethods.xTruncate(id, size);
        }

        private static int sqlite3OsSync(sqlite3_file id, int flags)
        {
            DO_OS_MALLOC_TEST(id);
            return id.pMethods.xSync(id, flags);
        }

        private static int sqlite3OsFileSize(sqlite3_file id, ref int pSize)
        {
            return id.pMethods.xFileSize(id, ref pSize);
        }

        private static int sqlite3OsLock(sqlite3_file id, int lockType)
        {
            DO_OS_MALLOC_TEST(id);
            return id.pMethods.xLock(id, lockType);
        }

        private static int sqlite3OsUnlock(sqlite3_file id, int lockType)
        {
            return id.pMethods.xUnlock(id, lockType);
        }

        private static int sqlite3OsCheckReservedLock(sqlite3_file id, ref int pResOut)
        {
            DO_OS_MALLOC_TEST(id);
            return id.pMethods.xCheckReservedLock(id, ref pResOut);
        }

        private static int sqlite3OsFileControl(sqlite3_file id, u32 op, ref int pArg)
        {
            return id.pMethods.xFileControl(id, (int)op, ref pArg);
        }

        private static int sqlite3OsSectorSize(sqlite3_file id)
        {
            var xSectorSize = id.pMethods.xSectorSize;
            return xSectorSize != null ? xSectorSize(id) : SQLITE_DEFAULT_SECTOR_SIZE;
        }

        private static int sqlite3OsDeviceCharacteristics(sqlite3_file id)
        {
            return id.pMethods.xDeviceCharacteristics(id);
        }

        /*
         ** The next group of routines are convenience wrappers around the
         ** VFS methods.
         */
        private static int sqlite3OsOpen(
            sqlite3_vfs pVfs,
            string zPath,
            sqlite3_file pFile,
            int flags,
            ref int pFlagsOut
        )
        {
            int rc;
            DO_OS_MALLOC_TEST(null);
            rc = pVfs.xOpen(pVfs, zPath, pFile, flags, ref pFlagsOut);
            Debug.Assert(rc == SQLITE_OK || pFile.pMethods == null);
            return rc;
        }

        private static int sqlite3OsDelete(sqlite3_vfs pVfs, string zPath, int dirSync)
        {
            return pVfs.xDelete(pVfs, zPath, dirSync);
        }

        private static int sqlite3OsAccess(sqlite3_vfs pVfs, string zPath, int flags, ref int pResOut)
        {
            DO_OS_MALLOC_TEST(null);
            return pVfs.xAccess(pVfs, zPath, flags, ref pResOut);
        }

        private static int sqlite3OsFullPathname(
            sqlite3_vfs pVfs,
            string zPath,
            int nPathOut,
            StringBuilder zPathOut
        )
        {
            return pVfs.xFullPathname(pVfs, zPath, nPathOut, zPathOut);
        }
#if !SQLITE_OMIT_LOAD_EXTENSION
        private static HANDLE sqlite3OsDlOpen(sqlite3_vfs pVfs, string zPath)
        {
            return pVfs.xDlOpen(pVfs, zPath);
        }

        private static void sqlite3OsDlError(sqlite3_vfs pVfs, int nByte, ref string zBufOut)
        {
            pVfs.xDlError(pVfs, nByte, ref zBufOut);
        }

        private static object sqlite3OsDlSym(sqlite3_vfs pVfs, HANDLE pHdle, ref string zSym)
        {
            return pVfs.xDlSym(pVfs, pHdle, zSym);
        }

        private static void sqlite3OsDlClose(sqlite3_vfs pVfs, HANDLE pHandle)
        {
            pVfs.xDlClose(pVfs, pHandle);
        }
#endif
        private static int sqlite3OsRandomness(sqlite3_vfs pVfs, int nByte, ref byte[] zBufOut)
        {
            return pVfs.xRandomness(pVfs, nByte, ref zBufOut);
        }

        private static int sqlite3OsSleep(sqlite3_vfs pVfs, int nMicro)
        {
            return pVfs.xSleep(pVfs, nMicro);
        }

        private static int sqlite3OsCurrentTime(sqlite3_vfs pVfs, ref double pTimeOut)
        {
            return pVfs.xCurrentTime(pVfs, ref pTimeOut);
        }

        private static int sqlite3OsOpenMalloc(
            ref sqlite3_vfs pVfs,
            string zFile,
            ref sqlite3_file ppFile,
            int flags,
            ref int pOutFlags
        )
        {
            var rc = SQLITE_NOMEM;
            sqlite3_file pFile;
            pFile = new sqlite3_file(); //sqlite3Malloc(ref pVfs.szOsFile);
            if (pFile != null)
            {
                rc = sqlite3OsOpen(pVfs, zFile, pFile, flags, ref pOutFlags);
                if (rc != SQLITE_OK)
                    pFile = null; // was  //sqlite3DbFree(db,ref  pFile);
                else
                    ppFile = pFile;
            }

            return rc;
        }

        private static int sqlite3OsCloseFree(sqlite3_file pFile)
        {
            var rc = SQLITE_OK;
            Debug.Assert(pFile != null);
            rc = sqlite3OsClose(pFile);
            //sqlite3_free( ref  pFile );
            return rc;
        }

        /*
         ** The list of all registered VFS implementations.
         */
        private static sqlite3_vfs vfsList;
        //#define vfsList GLOBAL(sqlite3_vfs *, vfsList)

        /*
         ** Locate a VFS by name.  If no name is given, simply return the
         ** first VFS on the list.
         */
        private static bool isInit = false;

        private static sqlite3_vfs sqlite3_vfs_find(string zVfs)
        {
            sqlite3_vfs pVfs = null;
#if SQLITE_THREADSAFE
sqlite3_mutex mutex;
#endif
#if !SQLITE_OMIT_AUTOINIT
            var rc = sqlite3_initialize();
            if (rc != 0) return null;
#endif
#if SQLITE_THREADSAFE
mutex = sqlite3MutexAlloc(SQLITE_MUTEX_STATIC_MASTER);
#endif
            sqlite3_mutex_enter(mutex);
            for (pVfs = vfsList; pVfs != null; pVfs = pVfs.pNext)
            {
                if (zVfs == null || zVfs == "") break;
                if (zVfs == pVfs.zName) break; //strcmp(zVfs, pVfs.zName) == null) break;
            }

            sqlite3_mutex_leave(mutex);
            return pVfs;
        }

        /*
         ** Unlink a VFS from the linked list
         */
        private static void vfsUnlink(sqlite3_vfs pVfs)
        {
            Debug.Assert(sqlite3_mutex_held(sqlite3MutexAlloc(SQLITE_MUTEX_STATIC_MASTER)));
            if (pVfs == null)
            {
                /* No-op */
            }
            else if (vfsList == pVfs)
            {
                vfsList = pVfs.pNext;
            }
            else if (vfsList != null)
            {
                var p = vfsList;
                while (p.pNext != null && p.pNext != pVfs) p = p.pNext;
                if (p.pNext == pVfs) p.pNext = pVfs.pNext;
            }
        }

        /*
         ** Register a VFS with the system.  It is harmless to register the same
         ** VFS multiple times.  The new VFS becomes the default if makeDflt is
         ** true.
         */
        private static int sqlite3_vfs_register(sqlite3_vfs pVfs, int makeDflt)
        {
            sqlite3_mutex mutex;
#if !SQLITE_OMIT_AUTOINIT
            var rc = sqlite3_initialize();
            if (rc != 0) return rc;
#endif
            mutex = sqlite3MutexAlloc(SQLITE_MUTEX_STATIC_MASTER);
            sqlite3_mutex_enter(mutex);
            vfsUnlink(pVfs);
            if (makeDflt != 0 || vfsList == null)
            {
                pVfs.pNext = vfsList;
                vfsList = pVfs;
            }
            else
            {
                pVfs.pNext = vfsList.pNext;
                vfsList.pNext = pVfs;
            }

            Debug.Assert(vfsList != null);
            sqlite3_mutex_leave(mutex);
            return SQLITE_OK;
        }

        /*
         ** Unregister a VFS so that it is no longer accessible.
         */
        private static int sqlite3_vfs_unregister(sqlite3_vfs pVfs)
        {
#if SQLITE_THREADSAFE
sqlite3_mutex mutex = sqlite3MutexAlloc(SQLITE_MUTEX_STATIC_MASTER);
#endif
            sqlite3_mutex_enter(mutex);
            vfsUnlink(pVfs);
            sqlite3_mutex_leave(mutex);
            return SQLITE_OK;
        }
    }
}