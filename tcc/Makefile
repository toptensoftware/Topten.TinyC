# This is the post build step called from the Visual Studio project
# to build the lib and include sub-directories.

CONFIG ?= Debug
PLATFORM ?= x64

TCCSRCDIR := ../tinycc
OBJDIR := ../bin/$(CONFIG)/$(PLATFORM)/obj
BASEDIR := ../bin/$(CONFIG)/Redist/$(PLATFORM)
LIBDIR := $(BASEDIR)/lib
INCLUDEDIR := $(BASEDIR)/include
TCC := $(BASEDIR)/tcc.exe

# libtcc1 C source
CSOURCES = \
	$(TCCSRCDIR)/lib/libtcc1.c \
	$(TCCSRCDIR)/lib/stdatomic.c \
	$(TCCSRCDIR)/win32/lib/crt1.c \
	$(TCCSRCDIR)/win32/lib/crt1w.c \
	$(TCCSRCDIR)/win32/lib/wincrt1.c \
	$(TCCSRCDIR)/win32/lib/wincrt1w.c \
	$(TCCSRCDIR)/win32/lib/dllcrt1.c \
	$(TCCSRCDIR)/win32/lib/dllmain.c

# libtcc1 AS files
ASSOURCES = \
	$(TCCSRCDIR)/lib/alloca.S \
	$(TCCSRCDIR)/lib/alloca-bt.S \
	$(TCCSRCDIR)/win32/lib/chkstk.S

# bcheck C files
BCHECKSOURCES = \
	$(TCCSRCDIR)/lib/bcheck.c \
	$(TCCSRCDIR)/lib/bt-exe.c \
	$(TCCSRCDIR)/lib/bt-log.c \
	$(TCCSRCDIR)/lib/bt-dll.c

# Work out object files
OBJS := $(addprefix $(OBJDIR)/,$(notdir $(CSOURCES:%.c=%.o) $(ASSOURCES:%.S=%.o)))
BCHECKOBJS := $(addprefix $(LIBDIR)/,$(notdir $(BCHECKSOURCES:%.c=%.o)))

# Setup VPATH to find source files
VPATH = $(sort $(dir $(ASSOURCES) $(CSOURCES) ))

# Build all this...
all: $(LIBDIR) $(INCLUDEDIR) $(LIBDIR)/libtcc1.a $(BCHECKOBJS)

# libtcc1 AR rule
$(LIBDIR)/libtcc1.a: $(OBJS)
	@echo "  AR    $(notdir $@)"
	@mkdir -p $(@D)
	@$(TCC) -B$(BASEDIR) -ar $@ $(OBJS)

# libtcc1 CC rule
$(OBJDIR)/%.o: %.c
	@echo "  CC    $(notdir $@)"
	@mkdir -p $(@D)
	@$(TCC) -B$(BASEDIR) -c $< -o $@

# libtcc1 AS rule
$(OBJDIR)/%.o: %.S
	@echo "  AS    $(notdir $@)"
	@mkdir -p $(@D)
	@$(TCC) -B$(BASEDIR) -c $< -o $@

# bcheck rule (adds -bt backtrace option)
$(LIBDIR)/bcheck.o: bcheck.c
	@echo "  CC    $(notdir $@)"
	@mkdir -p $(@D)
	@$(TCC) -B$(BASEDIR) -c $< -o $@ -bt

# other bcheck files
$(LIBDIR)/%.o: %.c
	@echo "  CC    $(notdir $@)"
	@mkdir -p $(@D)
	@$(TCC) -B$(BASEDIR) -c $< -o $@

# Copy include directory
$(INCLUDEDIR):
	@mkdir -p $@
	@cp $(TCCSRCDIR)/include/*.h $(INCLUDEDIR)
	@cp -r $(TCCSRCDIR)/win32/include/* $(INCLUDEDIR)
	@cp $(TCCSRCDIR)/tcclib.h $(INCLUDEDIR)
	@cp $(TCCSRCDIR)/libtcc.h $(INCLUDEDIR)

# Copy library def files
$(LIBDIR):
	@mkdir -p $@
	@cp $(TCCSRCDIR)/win32/lib/*.def $(LIBDIR)

# Clear
clean:
	@rm -rf $(INCLUDEDIR)
	@rm -rf $(OBJDIR)
	@rm -rf $(LIBDIR)
	

