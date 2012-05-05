#include <uv.h>
#include <stdio.h>
#include <stdlib.h>

#define UV_ERRNO_GEN(val, name, s) \
if (val != UV_UNKNOWN) { \
  printf("\t\tUV_%s = %u,\n", #name, val); \
} else { \
  printf("\t\tUV_UNKNOWN = -1,\n"); \
}

void print_err()
{
  printf("using System;\n\nnamespace LibuvSharp\n{\n\tinternal enum uv_err_code\n\t{\n");
  UV_ERRNO_MAP(UV_ERRNO_GEN)
  printf("\t}\n}\n");
}

void print_req()
{
  printf("using System;\n\nnamespace LibuvSharp\n{\n\tinternal enum UvRequestType : int\n\t{\n");
  printf("\t\tUV_UNKNOWN_REQ = 0,\n");
#define XX(uc, lc) printf("\t\tUV_%s,\n", #uc);
  UV_REQ_TYPE_MAP(XX);
#undef XX
  printf("\t\tUV_REQ_TYPE_PRIVATE,\n");
  printf("\t\tUV_REQ_TYPE_MAX,\n");
  printf("\t}\n}\n");
}

void print_handle()
{
  printf("using System;\n\nnamespace LibuvSharp\n{\n\tinternal enum UvHandleType : int\n\t{\n");
  printf("\t\tUV_UNKNOWN_HANDLE = 0,\n");
#define XX(uc, lc) printf("\t\tUV_%s,\n", #uc);
  UV_HANDLE_TYPE_MAP(XX);
#undef XX
  printf("\t\tUV_HANDLE_TYPE_PRIVATE,\n");
  printf("\t\tUV_HANDLE_TYPE_MAX,\n");
  printf("\t}\n}\n");
}

int main(int argc, char **argv)
{
  if (argc < 2) {
    printf("Provide at least on parameter (err, req, handle)");
    exit(0);
  }

  if (!strcmp(argv[1], "err")) {
    print_err();
  } else if (!strcmp(argv[1], "req")) {
    print_req();
  } else if (!strcmp(argv[1], "handle")) {
    print_handle();
  }
  return 0;
}
