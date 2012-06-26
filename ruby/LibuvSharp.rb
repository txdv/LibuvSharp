# make it easier to consume the LibuvSharp library with ruby

class LibuvSharp::Loop
  alias_method :base_sync, :sync

  def sync(*args, &block)
    args.push block if !block.nil?
    base_sync *args
  end
end

class LibuvSharp::Dns
  alias_method :base_resolve, :resolve

  def resolve(*args, &block)
    args.push block if !block.nil?
    base_resolve *args
  end
end

class LibuvSharp::Udp
  alias_method :base_send, :Send
  def Send(*args, &block)
    args.push block if !block.nil?
    base_send *args
  end

  alias_method :base_receive, :receive
  def receive(*args, &block)
    args.push block if !block.nil?
    base_receive *args
  end
end

class LibuvSharp::Timer
  alias_method :base_start, :start
  def start(*args, &block)
    args.push block if !block.nil?
    base_start *args
  end

  alias_method :running?, :running
  alias_method :is_running?, :running?
end

class LibuvSharp::Poll
  alias_method :base_start, :start
  def start(*args, &block)
    args.push block if !block.nil?
    base_start *args
  end
end
