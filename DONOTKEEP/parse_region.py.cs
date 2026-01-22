// Originally Python code written by TheMcSebi https://github.com/TheMcSebi/hytale-region-parser/blob/main/parse_region.py

using MinetaleConverter.Base;
using System.Text;
// BSON element types
public enum BsonType
{
    DOUBLE = 0x01,
    STRING = 0x02,
    DOCUMENT = 0x03,
    ARRAY = 0x04,
    BINARY = 0x05,
    UNDEFINED = 0x06, // Deprecated
    OBJECT_ID = 0x07,
    BOOLEAN = 0x08,
    DATETIME = 0x09,
    NULL = 0x0A,
    REGEX = 0x0B,
    DBPOINTER = 0x0C, // Deprecated
    JAVASCRIPT = 0x0D,
    SYMBOL = 0x0E, // Deprecated
    CODE_W_SCOPE = 0x0F,
    INT32 = 0x10,
    TIMESTAMP = 0x11,
    INT64 = 0x12,
    DECIMAL128 = 0x13,
    MIN_KEY = 0xFF,
    MAX_KEY = 0x7F
}

// Parser for BSON documents used by Hytale's codec system
public class BsonParser {
        
    public byte[] data;
        
    public int pos;
        
    public BsonParser(byte[] data) {
        this.data = data;
        this.pos = 0;
    }
        
    public virtual object remaining() {
        return this.data.Length - this.pos;
    }
        
    public virtual byte read_byte() {
        if (this.pos >= this.data.Length) {
            throw new Exception("Unexpected end of data");
        }
        var value = this.data[this.pos];
        this.pos += 1;
        return value;
    }
        
    public virtual byte[] read_bytes(int n) {
        if (this.pos + n > this.data.Length) {
            throw new Exception($"Not enough data: need {n}, have {this.remaining()}");
        }
        var value = this.data[this.pos..(this.pos  +  n)];
        this.pos += n;
        return value;
    }
        
    // Read little-endian 32-bit signed integer
    public virtual int read_int32() {
        var data = this.read_bytes(4);
        //return @struct.unpack("<i", data)[0];
        return BitConverter.ToInt32(data);
    }
        
    // Read big-endian 32-bit signed integer
    public virtual object read_int32_be() {
        var data = this.read_bytes(4);
        //return @struct.unpack(">i", data)[0];
        return data.SwapEndian();
        //return BitConverter.ToInt32(data);
    }
        
    // Read little-endian 32-bit unsigned integer
    public virtual object read_uint32() {
        var data = this.read_bytes(4);
        //return @struct.unpack("<I", data)[0];
        return BitConverter.ToUInt32(data);
    }
        
    // Read little-endian 64-bit signed integer
    public virtual long read_int64() {
        var data = this.read_bytes(8);
        //return @struct.unpack("<q", data)[0];
        return BitConverter.ToInt64(data);
    }
        
    // Read 64-bit IEEE 754 floating point
    public virtual double read_double() {
        var data = this.read_bytes(8);
        //return @struct.unpack("<d", data)[0];
        return BitConverter.ToDouble(data);
    }
        
    // Read null-terminated string
    public virtual string read_cstring() {
        //var end = this.data.find(new byte[] { 0x00 }, this.pos);
        var end = this.data.FindNextIndex(x => x == 0x00, this.pos);
        if (end == -1) {
            throw new Exception("Unterminated cstring");
        }
        var value = BitConverter.ToString(this.data[this.pos..(end - 1)]);
        //var value = this.data[this.pos..(end - 1)].decode("utf-8", errors: "replace");
        this.pos = end + 1;
        return value;
    }
        
    // Read BSON string (length-prefixed)
    public virtual string read_string() {
        var length = this.read_int32();
        if (length < 1) {
            throw new Exception($"Invalid string length: {length}");
        }
        var data = this.read_bytes(length);
        if (data[^1] != 0) {
            throw new Exception("String not null-terminated");
        }
        return data[: - 1:].decode("utf-8", errors: "replace");
    }
        
    // Read BSON binary data
    public virtual object read_binary() {
        var length = this.read_int32();
        var subtype = this.read_byte();
        var data = this.read_bytes(length);
        return (subtype, data);
    }
        
    // Read a BSON document
    public virtual Dictionary<string, object> read_document() {
        var start_pos = this.pos;
        var doc_size = this.read_int32();
        var result = new Dictionary<string, object>();
        while (this.pos < start_pos + doc_size - 1) {
            var element_type = this.read_byte();
            if (element_type == 0) {
                break;
            }
            var name = this.read_cstring();
            var value = this.read_element(element_type);
            result[name] = value;
        }
        // Read final null byte
        if (this.pos < start_pos + doc_size) {
            this.pos = start_pos + doc_size;
        }
        return result;
    }
        
    // Read a BSON array
    public virtual List<object> read_array() {
        var doc = this.read_document();
        // BSON arrays are documents with string indices "0", "1", "2", ...
        return (from i in Enumerable.Range(0, doc.Count)
            select doc[i.ToString()]).ToList();
    }
        
    // Read a BSON element value
    public virtual object read_element(byte element_type) {
        if (element_type == (byte)BsonType.DOUBLE) {
            return this.read_double();
        } else if (element_type == (byte)BsonType.STRING) {
            return this.read_string();
        } else if (element_type == (byte)BsonType.DOCUMENT) {
            return this.read_document();
        } else if (element_type == (byte)BsonType.ARRAY) {
            return this.read_array();
        } else if (element_type == (byte)BsonType.BINARY) {
            return this.read_binary();
        } else if (element_type == (byte)BsonType.BOOLEAN) {
            return this.read_byte() != 0;
        } else if (element_type == (byte)BsonType.NULL) {
            return null;
        } else if (element_type == (byte)BsonType.INT32) {
            return this.read_int32();
        } else if (element_type == (byte)BsonType.INT64) {
            return this.read_int64();
        } else if (element_type == (byte)BsonType.DATETIME) {
            return this.read_int64();
        } else if (element_type == (byte)BsonType.TIMESTAMP) {
            return this.read_int64();
        } else if (element_type == (byte)BsonType.OBJECT_ID) {
            return this.read_bytes(12).hex();
        } else if (element_type == (byte)BsonType.UNDEFINED) {
            return null;
        } else if (element_type == (byte)BsonType.REGEX) {
            var pattern = this.read_cstring();
            var options = this.read_cstring();
            return new Dictionary<object, object> {
                {
                    "pattern",
                    pattern},
                {
                    "options",
                    options}};
        } else {
            throw new Exception($"Unknown BSON type: {element_type:#x}");
        }
    }
        
    // Parse the entire BSON document
    public virtual object parse() {
        return this.read_document();
    }
}
    
// Represents a block component at a specific position
public class BlockComponent 
{
        
    public object data;
        
    public object index;
        
    public object position;
        
    public object component_type;
        
    public object data = field(default_factory: dict);
}
    
// Represents an item container (chest, etc.)
public class ItemContainerData 
{
        
    public int capacity = 0;
        
    public object position;
        
    public object items = field(default_factory: list);
        
    public bool allow_viewing = true;
        
    public void custom_name = null;
        
    public void who_placed_uuid = null;
        
    public bool placed_by_interaction = false;
}
    
// Represents a 32x32x32 chunk section
public class ChunkSectionData 
{ 
    public object section_y;
        
    public object block_palette = field(default_factory: list);
        
    public void block_data = null;
        
    public void filler_data = null;
        
    public void rotation_data = null;
        
    public void physics_data = null;
        
    public void fluid_data = null;
        
    public bool has_light_data = false;
}
    
// Complete parsed chunk data
public class ParsedChunkData {
        
    public object block_components;
        
    public object block_names;
        
    public int chunk_x;
        
    public int chunk_z;
        
    public object containers;
        
    public object entities;
        
    public void heightmap;
        
    public object raw_components;
        
    public object sections;
        
    public void tintmap;
        
    public int version;
        
    public int chunk_x = 0;
        
    public int chunk_z = 0;
        
    public int version = 0;
        
    public object sections = field(default_factory: list);
        
    public object block_components = field(default_factory: list);
        
    public object containers = field(default_factory: list);
        
    public object entities = field(default_factory: list);
        
    public object block_names = field(default_factory: set);
        
    public void heightmap = null;
        
    public void tintmap = null;
        
    public object raw_components = field(default_factory: dict);
}
    
// Parser for IndexedStorageFile format used by Hytale
public class IndexedStorageFile {
        
    public List<object> blob_count;
        
    public List<object> blob_indexes;
        
    public object filepath;
        
    public List<object> segment_size;
        
    public List<object> version;
        
    public object MAGIC_STRING = new byte[] { (byte)'H', (byte)'y', (byte)'t', (byte)'a', (byte)'l', (byte)'e', (byte)'I', (byte)'n', (byte)'d', (byte)'e', (byte)'x', (byte)'e', (byte)'d', (byte)'S', (byte)'t', (byte)'o', (byte)'r', (byte)'a', (byte)'g', (byte)'e' };
        
    public object MAGIC_LENGTH = 20;
        
    public object VERSION_OFFSET = 20;
        
    public object BLOB_COUNT_OFFSET = 24;
        
    public object SEGMENT_SIZE_OFFSET = 28;
        
    public object HEADER_LENGTH = 32;
        
    public object BLOB_HEADER_LENGTH = 8;
        
    public object SRC_LENGTH_OFFSET = 0;
        
    public object COMPRESSED_LENGTH_OFFSET = 4;
        
    public IndexedStorageFile(object filepath) {
        this.filepath = filepath;
        this.version = null;
        this.blob_count = null;
        this.segment_size = null;
        this.blob_indexes = new List<object>();
    }
        
    // Read and validate the file header
    public virtual bool read_header(object f) {
        f.seek(0);
        var header = f.read(this.HEADER_LENGTH);
        if (header.Count < this.HEADER_LENGTH) {
            Console.WriteLine($"Error: File too small, expected at least {self.HEADER_LENGTH} bytes");
            return false;
        }
        // Check magic
        var magic = header[:self.MAGIC_LENGTH:];
        if (magic != this.MAGIC_STRING) {
            Console.WriteLine($"Error: Invalid magic string. Expected {self.MAGIC_STRING}, got {magic}");
            return false;
        }
        // Read version
        this.version = @struct.unpack(">I", header[self.VERSION_OFFSET:(self.VERSION_OFFSET  +  4):])[0];
        if (this.version < 0 || this.version > 1) {
            Console.WriteLine($"Error: Unsupported version {self.version}");
            return false;
        }
        // Read blob count and segment size
        this.blob_count = @struct.unpack(">I", header[self.BLOB_COUNT_OFFSET:(self.BLOB_COUNT_OFFSET  +  4):])[0];
        this.segment_size = @struct.unpack(">I", header[self.SEGMENT_SIZE_OFFSET:(self.SEGMENT_SIZE_OFFSET  +  4):])[0];
        Console.WriteLine($"File: {self.filepath.name}");
        Console.WriteLine($"  Version: {self.version}");
        Console.WriteLine($"  Blob count: {self.blob_count}");
        Console.WriteLine($"  Segment size: {self.segment_size}");
        return true;
    }
        
    // Read the blob index table
    public virtual void read_blob_indexes(object f) {
        f.seek(this.HEADER_LENGTH);
        var index_data = f.read(this.blob_count * 4);
        this.blob_indexes = new List<object>();
        foreach (var i in Enumerable.Range(0, this.blob_count)) {
            var offset = i * 4;
            var segment_index = @struct.unpack(">I", index_data[offset:(offset  +  4):])[0];
            this.blob_indexes.append(segment_index);
        }
    }
        
    // Get the file position where segments start
    public virtual int segments_base() {
        return this.HEADER_LENGTH + this.blob_count * 4;
    }
        
    // Convert segment index to file position
    public virtual int segment_position(int segment_index) {
        if (segment_index == 0) {
            throw new ValueError("Invalid segment index 0");
        }
        var segment_offset = (segment_index - 1) * this.segment_size;
        return segment_offset + this.segments_base();
    }
        
    // Read and decompress a blob
    public virtual object read_blob(object f, int blob_index) {
        if (blob_index < 0 || blob_index >= this.blob_count) {
            throw new IndexError($"Blob index {blob_index} out of range");
        }
        var first_segment_index = this.blob_indexes[blob_index];
        if (first_segment_index == 0) {
            return null;
        }
        // Read blob header
        var pos = this.segment_position(first_segment_index);
        f.seek(pos);
        var blob_header = f.read(this.BLOB_HEADER_LENGTH);
        var src_length = @struct.unpack(">I", blob_header[self.SRC_LENGTH_OFFSET:(self.SRC_LENGTH_OFFSET  +  4):])[0];
        var compressed_length = @struct.unpack(">I", blob_header[self.COMPRESSED_LENGTH_OFFSET:(self.COMPRESSED_LENGTH_OFFSET  +  4):])[0];
        // Read compressed data
        var compressed_data = f.read(compressed_length);
        if (compressed_data.Count != compressed_length) {
            Console.WriteLine($"Warning: Expected {compressed_length} bytes, got {len(compressed_data)}");
            return null;
        }
        // Decompress
        try {
            var dctx = zstd.ZstdDecompressor();
            var decompressed = dctx.decompress(compressed_data, max_output_size: src_length);
            return decompressed;
        } catch (Exception) {
            Console.WriteLine($"Error decompressing blob {blob_index}: {e}");
            return null;
        }
    }
        
    // Convert blob index to chunk coordinates
    public virtual object get_chunk_coordinates(int blob_index, int region_x, int region_z) {
        // Each region is 32x32 chunks (1024 total)
        var local_x = blob_index % 32;
        var local_z = blob_index / 32;
        var chunk_x = region_x << 5 | local_x;
        var chunk_z = region_z << 5 | local_z;
        return (chunk_x, chunk_z);
    }
}
    
// Parser for chunk data in Hytale's custom codec format (BSON-based)
public class ChunkDataParser {
        
    public object data;
        
    public object pos;
        
    public object KNOWN_COMPONENTS = new HashSet {
        "WorldChunk",
        "BlockChunk",
        "BlockComponentChunk",
        "BlockComponents",
        "ChunkColumn",
        "ChunkSection",
        "EntityChunk",
        "EnvironmentChunk",
        "Block",
        "BlockPhysics",
        "FluidSection",
        "Fluid"
    };
        
    public object KNOWN_BLOCK_STATES = new HashSet {
        "ItemContainerState",
        "SignState",
        "BedState",
        "DoorState",
        "TrapDoorState",
        "FenceGateState",
        "LeverState",
        "ButtonState",
        "PressurePlateState",
        "TorchState",
        "LampState",
        "BellState"
    };
        
    public object BLOCK_NAME_PATTERN = re.compile(@"^([A-Z][a-z]+)_([A-Z][a-z][a-z0-9]*)(?:_[A-Z][a-z][a-z0-9]*)*$");
        
    public ChunkDataParser(object data) {
        this.data = data;
        this.pos = 0;
    }
        
    // Read a 4-byte big-endian integer
    public virtual int read_int() {
        if (this.pos + 4 > this.data.Count) {
            throw new ValueError("Not enough data to read int");
        }
        var value = @struct.unpack(">I", this.data[self.pos:(self.pos  +  4):])[0];
        this.pos += 4;
        return value;
    }
        
    // Read a 4-byte little-endian integer
    public virtual object read_int_le() {
        if (this.pos + 4 > this.data.Count) {
            throw new ValueError("Not enough data to read int");
        }
        var value = @struct.unpack("<i", this.data[self.pos:(self.pos  +  4):])[0];
        this.pos += 4;
        return value;
    }
        
    // Read a length-prefixed string (big-endian length)
    public virtual object read_string() {
        var length = this.read_int();
        if (length < 0 || length > 1000000) {
            // Sanity check
            throw new ValueError($"Invalid string length: {length}");
        }
        if (this.pos + length > this.data.Count) {
            throw new ValueError("Not enough data to read string");
        }
        var @string = this.data[self.pos:(self.pos  +  length):].decode("utf-8", errors: "replace");
        this.pos += length;
        return @string;
    }
        
    // Read a single byte
    public virtual int read_byte() {
        if (this.pos >= this.data.Count) {
            throw new ValueError("Not enough data to read byte");
        }
        var value = this.data[this.pos];
        this.pos += 1;
        return value;
    }
        
    // Read a specified number of bytes
    public virtual object read_bytes(int count) {
        if (this.pos + count > this.data.Count) {
            throw new ValueError("Not enough data to read bytes");
        }
        var value = this.data[self.pos:(self.pos  +  count):];
        this.pos += count;
        return value;
    }
        
    // Read a 2-byte big-endian short
    public virtual int read_short() {
        if (this.pos + 2 > this.data.Count) {
            throw new ValueError("Not enough data to read short");
        }
        var value = @struct.unpack(">H", this.data[self.pos:(self.pos  +  2):])[0];
        this.pos += 2;
        return value;
    }
        
    // Read a 2-byte little-endian short
    public virtual object read_short_le() {
        if (this.pos + 2 > this.data.Count) {
            throw new ValueError("Not enough data to read short");
        }
        var value = @struct.unpack("<h", this.data[self.pos:(self.pos  +  2):])[0];
        this.pos += 2;
        return value;
    }
        
    // Skip a specified number of bytes
    public virtual object skip_bytes(object count) {
        this.pos += count;
    }
        
    // Return number of bytes remaining
    public virtual object remaining() {
        return this.data.Count - this.pos;
    }
        
    // Try to parse the data as a BSON document
    public virtual object try_parse_bson() {
        try {
            var parser = new BsonParser(this.data);
            return parser.parse();
        } catch (Exception) {
            return null;
        }
    }
        
    // Extract block names using pattern matching on binary data
    public virtual object extract_block_names_from_bytes() {
        var block_names = new HashSet<object>();
        // Decode with replacement character for invalid sequences
        try {
            var text = this.data.decode("utf-8", errors: "replace");
        } catch {
            return block_names;
        }
        var VALID_PREFIXES = new HashSet {
            "Rock_",
            "Soil_",
            "Plant_",
            "Wood_",
            "Ore_",
            "Furniture_",
            "Rubble_",
            "Metal_",
            "Stone_",
            "Grass_",
            "Tree_",
            "Water_",
            "Lava_",
            "Ice_",
            "Sand_",
            "Brick_",
            "Glass_",
            "Cloth_",
            "Roof_",
            "Survival_",
            "Structure_",
            "Decor_",
            "Light_",
            "Fence_",
            "Wall_",
            "Floor_",
            "Stair_",
            "Door_",
            "Window_",
            "Chest_",
            "Barrel_",
            "Crate_",
            "Tool_",
            "Weapon_",
            "Crystal_",
            "Coral_",
            "Seaweed_",
            "Shell_",
            "Kelp_"
        };
        // Pattern: Capital + lowercase letters, then (_Capital + lowercase/digits)+
        var pattern = @"(?<![A-Za-z0-9_])([A-Z][a-z]+(?:_[A-Z][a-z][a-z0-9]*)+)(?![a-z])";
        foreach (var match in re.finditer(pattern, text)) {
            var name = match.group(1);
            // Skip if contains replacement character
            if (name.Contains("\ufffd")) {
                continue;
            }
            // Check if starts with valid prefix
            var has_valid_prefix = any(from prefix in VALID_PREFIXES
                select name.startswith(prefix));
            if (!has_valid_prefix) {
                continue;
            }
            // Verify each segment is properly formed
            var segments = name.split("_");
            var valid = true;
            foreach (var seg in segments) {
                if (seg.Count < 2 || !seg[0].isupper()) {
                    valid = false;
                    break;
                }
                var rest = seg[1];
                if (!rest || !all(from c in rest
                    select c.islower() || c.isdigit())) {
                    valid = false;
                    break;
                }
                if (!any(from c in rest
                    select c.islower())) {
                    valid = false;
                    break;
                }
            }
            if (valid && name.Count <= 80) {
                block_names.add(name);
            }
        }
        return block_names;
    }
        
    // Parse BlockComponentChunk/BlockComponents from BSON document
    public virtual object parse_block_components(object doc) {
        var components = new List<object>();
        // Look for BlockComponents in the document
        var block_components = doc.get("BlockComponents", new Dictionary<object, object> {
        });
        if (block_components is dict) {
            foreach (var (index_str, component_data) in block_components.items()) {
                try {
                    var index = Convert.ToInt32(index_str);
                    // Calculate position from index
                    // Index = x + y*32 + z*32*32 for a section
                    var x = index % 32;
                    var y = index / 32 % 32;
                    var z = index / (32 * 32);
                    // Determine component type
                    var comp_type = "Unknown";
                    if (component_data is dict) {
                        comp_type = component_data.get("Type", "Unknown");
                    }
                    var component = new BlockComponent(index: index, position: (x, y, z), component_type: comp_type, data: component_data is dict ? component_data : new Dictionary<object, object> {
                    });
                    components.append(component);
                } catch {
                    continue;
                }
            }
        }
        return components;
    }
        
    // Parse ItemContainer data from BSON document
    public virtual object parse_item_containers(object doc) {
        var containers = new List<object>();
        // Search recursively for ItemContainer data
        List<object> find_containers(object obj, string path = "") {
            result = new List<object>();
            if (obj is dict) {
                // Check for container component (nested in Components > container)
                inner_container = null;
                if (obj.Contains("Components") && obj["Components"] is dict) {
                    inner_container = obj["Components"].get("container");
                }
                // Check if this is an ItemContainerState or has ItemContainer directly
                container_obj = inner_container || obj;
                if (container_obj && container_obj is dict) {
                    if (container_obj.get("Type") == "ItemContainerState" || container_obj.Contains("ItemContainer")) {
                        pos = container_obj.get("Position", new Dictionary<object, object> {
                        });
                        if (pos is dict) {
                            position = (pos.get("X", 0), pos.get("Y", 0), pos.get("Z", 0));
                        } else {
                            position = (0, 0, 0);
                        }
                        item_container = container_obj.get("ItemContainer", new Dictionary<object, object> {
                        });
                        capacity = item_container is dict ? item_container.get("Capacity", 0) : 0;
                        container = new ItemContainerData(position: position, capacity: capacity, allow_viewing: container_obj.get("AllowViewing", true), custom_name: container_obj.get("Custom_Name"), who_placed_uuid: container_obj.get("WhoPlacedUuid"), placed_by_interaction: container_obj.get("PlacedByInteraction", false));
                        // Parse items if present
                        if (item_container is dict) {
                            items = item_container.get("Items", new Dictionary<object, object> {
                            });
                            // Items can be a dict with slot numbers as keys
                            if (items is dict) {
                                container.items = items.values().ToList();
                            } else if (items is list) {
                                container.items = items;
                            }
                        }
                        result.append(container);
                    }
                }
                // Recurse into dict values (but avoid double-counting)
                foreach (var (key, value) in obj.items()) {
                    if (!ValueTuple.Create("Components").Contains(key)) {
                        // Don't recurse into nested components we already handled
                        result.extend(find_containers(value, $"{path}.{key}"));
                    }
                }
            } else if (obj is list) {
                foreach (var (i, item) in obj.Select((_p_1,_p_2) => Tuple.Create(_p_2, _p_1))) {
                    result.extend(find_containers(item, $"{path}[{i}]"));
                }
            }
            return result;
        }
        return find_containers(doc);
    }
        
    // Parse chunk data and extract all components
    public virtual object parse() {
        object position;
        var result = new ParsedChunkData();
        // First try BSON parsing
        var bson_doc = this.try_parse_bson();
        if (bson_doc) {
            result.raw_components = bson_doc;
            // Extract version if present
            result.version = bson_doc.get("Version", 0);
            // Navigate to the Components section
            var components = bson_doc.get("Components", new Dictionary<object, object> {
            });
            // Parse block components from BlockComponentChunk
            var block_comp_chunk = components.get("BlockComponentChunk", new Dictionary<object, object> {
            });
            var block_components = block_comp_chunk.get("BlockComponents", new Dictionary<object, object> {
            });
            foreach (var (index_str, component_data) in block_components.items()) {
                try {
                    var index = Convert.ToInt32(index_str);
                    // Calculate position from index (within a 32x32 column)
                    var x = index % 32;
                    var y = index / 32 % 320;
                    var z = index / (32 * 320);
                    // Get the inner Components dict
                    var inner_comps = component_data is dict ? component_data.get("Components", new Dictionary<object, object> {
                    }) : new Dictionary<object, object> {
                    };
                    // Check for container
                    var container_data = inner_comps.get("container");
                    if (container_data && container_data is dict) {
                        var pos = container_data.get("Position", new Dictionary<object, object> {
                        });
                        if (pos is dict) {
                            position = (pos.get("X", 0), pos.get("Y", 0), pos.get("Z", 0));
                        } else {
                            position = (x, y, z);
                        }
                        var item_container = container_data.get("ItemContainer", new Dictionary<object, object> {
                        });
                        var capacity = item_container is dict ? item_container.get("Capacity", 0) : 0;
                        var container = new ItemContainerData(position: position, capacity: capacity, allow_viewing: container_data.get("AllowViewing", true), custom_name: container_data.get("Custom_Name"), who_placed_uuid: container_data.get("WhoPlacedUuid"), placed_by_interaction: container_data.get("PlacedByInteraction", false));
                        // Parse items if present
                        if (item_container is dict) {
                            var items = item_container.get("Items", new Dictionary<object, object> {
                            });
                            if (items is dict) {
                                container.items = items.values().ToList();
                            } else if (items is list) {
                                container.items = items;
                            }
                        }
                        result.containers.append(container);
                    }
                    // Check for other component types
                    foreach (var (comp_name, comp_data) in inner_comps.items()) {
                        var component = new BlockComponent(index: index, position: (x, y, z), component_type: comp_name, data: comp_data is dict ? comp_data : new Dictionary<object, object> {
                        });
                        result.block_components.append(component);
                    }
                } catch {
                    continue;
                }
            }
            // Extract entities if present
            var entity_chunk = components.get("EntityChunk", new Dictionary<object, object> {
            });
            if (entity_chunk is dict) {
                var entities = entity_chunk.get("Entities", new List<object>());
                if (entities is list) {
                    result.entities = entities;
                }
            }
        }
        // Also extract block names from raw bytes (for palette data)
        result.block_names = this.extract_block_names_from_bytes();
        return result;
    }
        
    // Parse a single block section
    public virtual object parse_block_section(object section_index) {
        var section = new ChunkSectionData(section_y: section_index);
        try {
            // Read palette type
            var palette_type = this.read_byte();
            // Read block palette based on type
            if (palette_type == 0) {
                // Empty
            } else if (palette_type == 1) {
                // Single value
                var block_id = this.read_int();
                section.block_palette = new List<string> {
                    block_id.ToString()
                };
            } else if (palette_type == 2) {
                // Indexed palette
                var palette_size = this.read_short();
                foreach (var _ in Enumerable.Range(0, palette_size)) {
                    // Read UTF string for block name
                    var str_len = this.read_short();
                    var block_name = this.read_bytes(str_len).decode("utf-8", errors: "replace");
                    section.block_palette.append(block_name);
                }
            }
        } catch (Exception) {
        }
        return section;
    }
}
    
// Parser for .region.bin files
public class RegionFileParser {
        
    public object filepath;
        
    public int region_x;
        
    public int region_z;
        
    public IndexedStorageFile storage;
        
    public RegionFileParser(object filepath) {
        this.filepath = filepath;
        this.storage = new IndexedStorageFile(filepath);
        this.region_x = null;
        this.region_z = null;
    }
        
    // Extract region coordinates from filename
    public virtual bool parse_filename() {
        var filename = this.filepath.stem;
        var parts = filename.split(".");
        if (parts.Count != 3 || parts[2] != "region") {
            Console.WriteLine($"Error: Invalid filename format. Expected X.Z.region.bin");
            return false;
        }
        try {
            this.region_x = Convert.ToInt32(parts[0]);
            this.region_z = Convert.ToInt32(parts[1]);
            Console.WriteLine($"\nRegion coordinates: ({self.region_x}, {self.region_z})");
            return true;
        } catch (ValueError) {
            Console.WriteLine($"Error: Could not parse region coordinates from filename");
            return false;
        }
    }
        
    // Parse the region file and list all chunks
    public virtual void parse() {
        if (!this.parse_filename()) {
            return;
        }
        using (var f = open(this.filepath, "rb")) {
            if (!this.storage.read_header(f)) {
                return;
            }
            this.storage.read_blob_indexes(f);
            // Find all chunks with data
            chunks_with_data = new List<object>();
            foreach (var blob_index in Enumerable.Range(0, this.storage.blob_count)) {
                if (this.storage.blob_indexes[blob_index] != 0) {
                    chunks_with_data.append(blob_index);
                }
            }
            Console.WriteLine($"\nChunks with data: {len(chunks_with_data)}/{self.storage.blob_count}");
            Console.WriteLine("\nChunk list:");
            Console.WriteLine("-" * 80);
            foreach (var blob_index in chunks_with_data) {
                (chunk_x, chunk_z) = this.storage.get_chunk_coordinates(blob_index, this.region_x, this.region_z);
                // Read the chunk data
                chunk_data = this.storage.read_blob(f, blob_index);
                if (chunk_data) {
                    Console.WriteLine($"Chunk ({chunk_x:4d}, {chunk_z:4d}) - Blob {blob_index:4d} - Size: {len(chunk_data):8d} bytes");
                    // Try to analyze the chunk data
                    this.analyze_chunk_data(chunk_data, chunk_x, chunk_z);
                } else {
                    Console.WriteLine($"Chunk ({chunk_x:4d}, {chunk_z:4d}) - Blob {blob_index:4d} - Failed to read");
                }
            }
        }
    }
        
    // Parse the region file and show a summary of all blocks
    public virtual void parse_summary() {
        if (!this.parse_filename()) {
            return;
        }
        var all_blocks = new Dictionary<object, object> {
        };
        var all_containers = new List<object>();
        var all_components = new List<object>();
        using (var f = open(this.filepath, "rb")) {
            if (!this.storage.read_header(f)) {
                return;
            }
            this.storage.read_blob_indexes(f);
            // Find all chunks with data
            chunks_with_data = new List<object>();
            foreach (var blob_index in Enumerable.Range(0, this.storage.blob_count)) {
                if (this.storage.blob_indexes[blob_index] != 0) {
                    chunks_with_data.append(blob_index);
                }
            }
            Console.WriteLine($"\nRegion ({self.region_x}, {self.region_z})");
            Console.WriteLine($"Chunks with data: {len(chunks_with_data)}/{self.storage.blob_count}");
            Console.WriteLine("\nProcessing chunks...");
            foreach (var (i, blob_index) in chunks_with_data.Select((_p_1,_p_2) => Tuple.Create(_p_2, _p_1))) {
                if (i % 100 == 0) {
                    Console.WriteLine($"  Progress: {i}/{len(chunks_with_data)} chunks processed");
                }
                chunk_data = this.storage.read_blob(f, blob_index);
                (chunk_x, chunk_z) = this.storage.get_chunk_coordinates(blob_index, this.region_x, this.region_z);
                if (chunk_data) {
                    parser = new ChunkDataParser(chunk_data);
                    try {
                        result = parser.parse();
                        // Collect block names
                        foreach (var block_name in result.block_names) {
                            all_blocks[block_name] = all_blocks.get(block_name, 0) + 1;
                        }
                        // Collect containers
                        foreach (var container in result.containers) {
                            container_info = new Dictionary<object, object> {
                                {
                                    "chunk",
                                    (chunk_x, chunk_z)},
                                {
                                    "position",
                                    container.position},
                                {
                                    "capacity",
                                    container.capacity},
                                {
                                    "items_count",
                                    container.items.Count}};
                            all_containers.append(container_info);
                        }
                        // Collect block components
                        foreach (var component in result.block_components) {
                            comp_info = new Dictionary<object, object> {
                                {
                                    "chunk",
                                    (chunk_x, chunk_z)},
                                {
                                    "position",
                                    component.position},
                                {
                                    "type",
                                    component.component_type},
                                {
                                    "index",
                                    component.index}};
                            all_components.append(comp_info);
                        }
                    } catch (Exception) {
                    }
                }
            }
        }
        // Print summary
        Console.WriteLine($"\n{'='*80}");
        Console.WriteLine($"SUMMARY - Region ({self.region_x}, {self.region_z})");
        Console.WriteLine($"{'='*80}");
        // Block types
        Console.WriteLine($"\nTotal unique block types: {len(all_blocks)}");
        Console.WriteLine("\nAll blocks (sorted by occurrence count):");
        Console.WriteLine("-" * 80);
        var sorted_blocks = all_blocks.items().OrderBy(x => -x[1]).ToList();
        foreach (var (block_type, count) in sorted_blocks) {
            Console.WriteLine($"  {block_type}: {count} occurrences");
        }
        // Group by category
        Console.WriteLine($"\n{'='*80}");
        Console.WriteLine("Blocks by Category:");
        Console.WriteLine($"{'='*80}");
        var categories = new Dictionary<object, object> {
        };
        foreach (var block_type in all_blocks.keys()) {
            if (block_type.Contains("_")) {
                var category = block_type.split("_")[0];
                if (!categories.Contains(category)) {
                    categories[category] = new List<object>();
                }
                categories[category].append(block_type);
            }
        }
        foreach (var category in categories.keys().OrderBy(_p_4 => _p_4).ToList()) {
            var blocks_in_category = categories[category].OrderBy(_p_5 => _p_5).ToList();
            Console.WriteLine($"\n{category} ({len(blocks_in_category)} types):");
            foreach (var block_type in blocks_in_category) {
                Console.WriteLine($"  - {block_type} ({all_blocks[block_type]} occurrences)");
            }
        }
        // Print containers if any
        if (all_containers) {
            Console.WriteLine($"\n{'='*80}");
            Console.WriteLine($"ITEM CONTAINERS ({len(all_containers)} total):");
            Console.WriteLine($"{'='*80}");
            foreach (var container in all_containers[:50:]) {
                // Limit output
                Console.WriteLine("  Chunk {container['chunk']}, Position {container['position']}, Capacity: {container['capacity']}, Items: {container['items_count']}");
            }
            if (all_containers.Count > 50) {
                Console.WriteLine($"  ... and {len(all_containers) - 50} more containers");
            }
        }
        // Print block components if any
        if (all_components) {
            Console.WriteLine($"\n{'='*80}");
            Console.WriteLine($"BLOCK COMPONENTS ({len(all_components)} total):");
            Console.WriteLine($"{'='*80}");
            // Group by type
            var comp_by_type = new Dictionary<object, object> {
            };
            foreach (var comp in all_components) {
                var comp_type = comp["type"];
                if (!comp_by_type.Contains(comp_type)) {
                    comp_by_type[comp_type] = new List<object>();
                }
                comp_by_type[comp_type].append(comp);
            }
            foreach (var (comp_type, comps) in comp_by_type.items().OrderBy(_p_6 => _p_6).ToList()) {
                Console.WriteLine($"\n  {comp_type}: {len(comps)} instances");
                foreach (var comp in comps[:10:]) {
                    Console.WriteLine($"    - Chunk {comp['chunk']}, Position {comp['position']}");
                }
                if (comps.Count > 10) {
                    Console.WriteLine($"    ... and {len(comps) - 10} more");
                }
            }
        }
    }
        
    // Parse with detailed BSON structure output for debugging
    public virtual void parse_detailed(int max_chunks = 5) {
        if (!this.parse_filename()) {
            return;
        }
        using (var f = open(this.filepath, "rb")) {
            if (!this.storage.read_header(f)) {
                return;
            }
            this.storage.read_blob_indexes(f);
            // Find all chunks with data
            chunks_with_data = new List<object>();
            foreach (var blob_index in Enumerable.Range(0, this.storage.blob_count)) {
                if (this.storage.blob_indexes[blob_index] != 0) {
                    chunks_with_data.append(blob_index);
                }
            }
            Console.WriteLine($"\nAnalyzing first {max_chunks} chunks in detail...");
            Console.WriteLine("-" * 80);
            foreach (var (i, blob_index) in chunks_with_data[:max_chunks:].Select((_p_1,_p_2) => Tuple.Create(_p_2, _p_1))) {
                (chunk_x, chunk_z) = this.storage.get_chunk_coordinates(blob_index, this.region_x, this.region_z);
                chunk_data = this.storage.read_blob(f, blob_index);
                if (chunk_data) {
                    Console.WriteLine($"\n{'='*80}");
                    Console.WriteLine($"CHUNK ({chunk_x}, {chunk_z}) - Blob {blob_index}");
                    Console.WriteLine($"Data size: {len(chunk_data)} bytes");
                    Console.WriteLine($"{'='*80}");
                    parser = new ChunkDataParser(chunk_data);
                    result = parser.parse();
                    // Print raw BSON structure if available
                    if (result.raw_components) {
                        Console.WriteLine("\nBSON Document Structure:");
                        this._print_bson_structure(result.raw_components, indent: 2);
                    }
                    // Print block names found
                    if (result.block_names) {
                        Console.WriteLine($"\nBlock names found: {len(result.block_names)}");
                        foreach (var name in result.block_names.OrderBy(_p_3 => _p_3).ToList()[:20:]) {
                            Console.WriteLine($"  - {name}");
                        }
                        if (result.block_names.Count > 20) {
                            Console.WriteLine($"  ... and {len(result.block_names) - 20} more");
                        }
                    }
                    // Print components
                    if (result.block_components) {
                        Console.WriteLine($"\nBlock components: {len(result.block_components)}");
                        foreach (var comp in result.block_components[:5:]) {
                            Console.WriteLine($"  - {comp.component_type} at {comp.position}");
                        }
                    }
                    // Print containers
                    if (result.containers) {
                        Console.WriteLine($"\nContainers: {len(result.containers)}");
                        foreach (var cont in result.containers[:5:]) {
                            Console.WriteLine($"  - Position {cont.position}, Capacity: {cont.capacity}");
                        }
                    }
                }
            }
        }
    }
        
    // Print BSON structure recursively
    public virtual void _print_bson_structure(object obj, int indent = 0) {
        var prefix = " " * indent;
        if (obj is dict) {
            foreach (var (key, value) in obj.items().ToList()[:20:]) {
                // Limit keys shown
                if (value is dict) {
                    Console.WriteLine($"{prefix}{key}: {{");
                    this._print_bson_structure(value, indent + 2);
                    Console.WriteLine($"{prefix}}}");
                } else if (value is list) {
                    Console.WriteLine($"{prefix}{key}: [{len(value)} items]");
                    if (value && value.Count <= 3) {
                        foreach (var item in value) {
                            this._print_bson_structure(item, indent + 2);
                        }
                    }
                } else if (value is bytes) {
                    Console.WriteLine($"{prefix}{key}: <binary {len(value)} bytes>");
                } else if (value is tuple && value.Count == 2) {
                    // Binary with subtype
                    Console.WriteLine($"{prefix}{key}: <binary subtype={value[0]}, {len(value[1])} bytes>");
                } else {
                    var val_str = value.ToString();
                    if (val_str.Count > 50) {
                        val_str = val_str[:50:] + "...";
                    }
                    Console.WriteLine($"{prefix}{key}: {val_str}");
                }
            }
            if (obj.Count > 20) {
                Console.WriteLine($"{prefix}... and {len(obj) - 20} more keys");
            }
        } else if (obj is list) {
            foreach (var (i, item) in obj[:5:].Select((_p_1,_p_2) => Tuple.Create(_p_2, _p_1))) {
                Console.WriteLine($"{prefix}[{i}]:");
                this._print_bson_structure(item, indent + 2);
            }
            if (obj.Count > 5) {
                Console.WriteLine($"{prefix}... and {len(obj) - 5} more items");
            }
        } else {
            Console.WriteLine($"{prefix}{obj}");
        }
    }
        
    // Attempt to analyze chunk data structure
    public virtual object analyze_chunk_data(object data, int chunk_x, int chunk_z) {
        var parser = new ChunkDataParser(data);
        try {
            var result = parser.parse();
            // Display blocks
            if (result.block_names) {
                Console.WriteLine($"  Blocks found: {len(result.block_names)} unique types");
                // Show top blocks
                var blocks_sorted = result.block_names.OrderBy(_p_1 => _p_1).ToList();
                foreach (var block in blocks_sorted[:20:]) {
                    Console.WriteLine($"    - {block}");
                }
                if (result.block_names.Count > 20) {
                    Console.WriteLine($"    ... and {len(result.block_names) - 20} more block types");
                }
            }
            // Display components
            if (result.block_components) {
                Console.WriteLine($"  Block components: {len(result.block_components)}");
                foreach (var comp in result.block_components[:5:]) {
                    Console.WriteLine($"    - {comp.component_type} at position {comp.position}");
                }
            }
            // Display containers
            if (result.containers) {
                Console.WriteLine($"  Containers: {len(result.containers)}");
                foreach (var container in result.containers[:5:]) {
                    Console.WriteLine($"    - Position {container.position}, Capacity: {container.capacity}");
                }
            }
        } catch (Exception) {
            Console.WriteLine($"  Error parsing chunk data: {e}");
            traceback.print_exc();
        }
        Console.WriteLine();
    }
        
    // Find printable ASCII strings in binary data
    public virtual object find_printable_strings(object data, object min_length = 4) {
        var strings = new List<object>();
        var current = new List<object>();
        foreach (var byte in data) {
            if (32 <= byte && byte <= 126) {
                // Printable ASCII
                current.append(chr(byte));
            } else {
                if (current.Count >= min_length) {
                    strings.append("".join(current));
                }
                current = new List<object>();
            }
        }
        if (current.Count >= min_length) {
            strings.append("".join(current));
        }
        return strings;
    }
}
    
//!/usr/bin/env python3
// ============================================================================
// BSON Parser for Hytale's Codec Format
// ============================================================================
// ============================================================================
// Data Classes for Parsed Chunk Data
// ============================================================================
// ============================================================================
// IndexedStorageFile Parser
// ============================================================================
// ============================================================================
// Chunk Data Parser
// ============================================================================
// ============================================================================
// Region File Parser
// ============================================================================
// ============================================================================
// Main Entry Point
// ============================================================================
public static void main() {
    if (sys.argv.Count < 2) {
        Console.WriteLine("Usage: python parse_region.py <region_file.bin> [options]");
        Console.WriteLine("\nExample: python parse_region.py chunks/0.0.region.bin");
        Console.WriteLine("         python parse_region.py chunks/0.0.region.bin --summary");
        Console.WriteLine("         python parse_region.py chunks/0.0.region.bin --detailed");
        Console.WriteLine("\nOptions:");
        Console.WriteLine("  --summary    Show summary of all unique blocks, containers, and components");
        Console.WriteLine("  --detailed   Show detailed BSON structure of first few chunks");
        sys.exit(1);
    }
    var filepath = Path(sys.argv[1]);
    if (!filepath.exists()) {
        Console.WriteLine($"Error: File not found: {filepath}");
        sys.exit(1);
    }
    var parser = new RegionFileParser(filepath);
    if (sys.argv.Contains("--summary")) {
        parser.parse_summary();
    } else if (sys.argv.Contains("--detailed")) {
        parser.parse_detailed();
    } else {
        parser.parse();
    }
}
    
static parse_region() {
    if (@__name__ == "__main__") {
    }
}